using HRMS.Data;
using HRMS.Models;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace HRMS.Jobs
{
    public class MonthlyAttendanceJob : IJob
    {
        private readonly ApplicationDbContext _context;

        public MonthlyAttendanceJob(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            int year = DateTime.Now.Year;
            int month = DateTime.Now.Month + 1;

            if (month == 13)
            {
                month = 1;
                year++;
            }

            var employees = await _context.Employees
                .Where(e => e.Status == "Active")
                .ToListAsync();

            var holidays = await _context.Holidays
                .Where(h => h.HolidayDate.Month == month && h.HolidayDate.Year == year)
                .Select(h => h.HolidayDate.Date)
                .ToListAsync();

            int daysInMonth = DateTime.DaysInMonth(year, month);

            foreach (var emp in employees)
            {
                // 🔹 Determine start date based on joining date
                DateTime monthStartDate = new DateTime(year, month, 1);
                 DateTime joiningDate = emp.JoiningDate?.Date ?? DateTime.MinValue;

                 DateTime startDate = joiningDate > monthStartDate
                    ? joiningDate
                    : monthStartDate;

                for (DateTime date = startDate; date.Month == month; date = date.AddDays(1))
                {
                    bool exists = await _context.Attendances.AnyAsync(a =>
                        a.Emp_Code == emp.EmployeeCode && a.Date == date);

                    if (exists) continue;

                    string status = "A";

                    if (date.DayOfWeek == DayOfWeek.Sunday)
                        status = "WO";
                    else if (holidays.Contains(date))
                        status = "HO";

                    _context.Attendances.Add(new Attendance
                    {
                        Emp_Code = emp.EmployeeCode,
                        Date = date,
                        Att_Date = date,
                        Status = status,
                        IsLate = false,
                        LateMinutes = 0,
                        Total_Hours = 0,
                        CorrectionRequested = false,
                        IsGeoAttendance = false
                    });
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}

//using HRMS.Data;
//using HRMS.Models;
//using Microsoft.EntityFrameworkCore;
//using Quartz;

//namespace HRMS.Jobs
//{
//    public class MonthlyAttendanceJob : IJob
//    {
//        private readonly ApplicationDbContext _context;

//        public MonthlyAttendanceJob(ApplicationDbContext context)
//        {
//            _context = context;
//        }

//        public async Task Execute(IJobExecutionContext context)
//        {
//            int year = DateTime.Now.Year;
//            int month = DateTime.Now.Month + 1;

//            if (month == 13)
//            {
//                month = 1;
//                year++;
//            }

//            var employees = await _context.Employees
//                .Where(e => e.Status == "Active")
//                .ToListAsync();

//            var holidays = await _context.Holidays
//                .Where(h => h.HolidayDate.Month == month && h.HolidayDate.Year == year)
//                .Select(h => h.HolidayDate.Date)
//                .ToListAsync();

//            int daysInMonth = DateTime.DaysInMonth(year, month);

//            foreach (var emp in employees)
//            {
//                for (int day = 1; day <= daysInMonth; day++)
//                {
//                    DateTime date = new DateTime(year, month, day);

//                    bool exists = await _context.Attendances.AnyAsync(a =>
//                        a.Emp_Code == emp.EmployeeCode && a.Date == date);

//                    if (exists) continue;

//                    string status = "A";

//                    if (date.DayOfWeek == DayOfWeek.Sunday)
//                        status = "WO";
//                    else if (holidays.Contains(date))
//                        status = "HO";

//                    _context.Attendances.Add(new Attendance
//                    {
//                        Emp_Code = emp.EmployeeCode,
//                        Date = date,
//                        Att_Date = date,
//                        Status = status,
//                        IsLate = false,
//                        LateMinutes = 0,
//                        Total_Hours = 0,
//                        CorrectionRequested = false,
//                        IsGeoAttendance = false
//                    });
//                }
//            }

//            await _context.SaveChangesAsync();
//        }
//    }
//}
