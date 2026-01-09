using HRMS.Data;
using HRMS.Models;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Services
{
    public interface ICompOffService
    {
        Task CreditCompOffIfApplicableAsync(int employeeId, DateTime date, string? remarks = null);
        Task<int> GetActiveBalanceAsync(int employeeId);
        Task<bool> ReserveAndUseCompOffAsync(int employeeId, int requiredDays, int leaveId);
        Task<int> ExpireOldCompOffAsync(DateTime today);
        Task<List<CompOffLedger>> GetHistoryAsync(int employeeId);
    }

    public class CompOffService : ICompOffService
    {
        private readonly ApplicationDbContext _context;

        public CompOffService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ POINT 1 + 2: Credit comp-off on Sunday OR Holiday + prevent duplicate for same day
        public async Task CreditCompOffIfApplicableAsync(int employeeId, DateTime date, string? remarks = null)
        {
            date = date.Date;

            bool isSunday = date.DayOfWeek == DayOfWeek.Sunday;

            // ✅ Holiday rule: attendance date exists in Holidays table
            // If you don’t have Holidays table, skip or adapt.
            bool isHoliday = await _context.Holidays.AnyAsync(h => h.HolidayDate.Date == date);

            if (!isSunday && !isHoliday)
                return;

            // ✅ Prevent duplicate credit for same day
            bool alreadyCredited = await _context.CompOffLedgers.AnyAsync(x =>
                x.EmployeeId == employeeId &&
                x.EarnedDate.Date == date);

            if (alreadyCredited)
                return;

            var expiry = date.AddDays(30);

            _context.CompOffLedgers.Add(new CompOffLedger
            {
                EmployeeId = employeeId,
                EarnedDate = date,
                ExpiryDate = expiry,
                Status = "Active",
                Remarks = remarks ?? (isSunday ? "Earned on Sunday" : "Earned on Holiday")
            });

            // Keep Employee.CompOffBalance in sync (optional but fast UI)
            var emp = await _context.Employees.FindAsync(employeeId);
            if (emp != null)
            {
                emp.CompOffBalance += 1;
                emp.LastCompOffEarnedDate = date;
            }

            await _context.SaveChangesAsync();
        }

        // Active balance from ledger
        public async Task<int> GetActiveBalanceAsync(int employeeId)
        {
            return await _context.CompOffLedgers.CountAsync(x =>
                x.EmployeeId == employeeId && x.Status == "Active");
        }

        // ✅ Use comp-offs when leave becomes finally approved
        public async Task<bool> ReserveAndUseCompOffAsync(int employeeId, int requiredDays, int leaveId)
        {
            // Ensure expiry is applied before usage
            await ExpireOldCompOffAsync(DateTime.Today);

            var active = await _context.CompOffLedgers
                .Where(x => x.EmployeeId == employeeId && x.Status == "Active")
                .OrderBy(x => x.EarnedDate)
                .Take(requiredDays)
                .ToListAsync();

            if (active.Count < requiredDays)
                return false;

            var today = DateTime.Today;

            foreach (var row in active)
            {
                row.Status = "Used";
                row.UsedDate = today;
                row.UsedLeaveId = leaveId;
            }

            var emp = await _context.Employees.FindAsync(employeeId);
            if (emp != null)
            {
                emp.CompOffBalance = await GetActiveBalanceAsync(employeeId) - requiredDays;
                // safer recompute after save below; we’ll fix after SaveChanges
            }

            await _context.SaveChangesAsync();

            // recompute exact
            if (emp != null)
            {
                emp.CompOffBalance = await GetActiveBalanceAsync(employeeId);
                await _context.SaveChangesAsync();
            }

            return true;
        }

        // ✅ POINT 4: Expire after 30 days
        public async Task<int> ExpireOldCompOffAsync(DateTime today)
        {
            today = today.Date;

            var expired = await _context.CompOffLedgers
                .Where(x => x.Status == "Active" && x.ExpiryDate.Date < today)
                .ToListAsync();

            if (expired.Count == 0) return 0;

            foreach (var row in expired)
                row.Status = "Expired";

            await _context.SaveChangesAsync();

            // Update all employees’ CompOffBalance quickly (optional)
            // If you prefer only per-employee updates, skip this loop and compute on demand.
            var employeeIds = expired.Select(e => e.EmployeeId).Distinct().ToList();
            foreach (var empId in employeeIds)
            {
                var emp = await _context.Employees.FindAsync(empId);
                if (emp != null)
                    emp.CompOffBalance = await GetActiveBalanceAsync(empId);
            }

            await _context.SaveChangesAsync();
            return expired.Count;
        }

        // ✅ POINT 3: History
        public async Task<List<CompOffLedger>> GetHistoryAsync(int employeeId)
        {
            return await _context.CompOffLedgers
                .Where(x => x.EmployeeId == employeeId)
                .OrderByDescending(x => x.EarnedDate)
                .ToListAsync();
        }

        // If you don’t have Holidays table -> remove Holiday check above OR create Holidays table.
    }
}
