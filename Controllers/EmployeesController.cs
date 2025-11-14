using HRMS.Data;
using HRMS.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        private static readonly Dictionary<string, List<string>> DepartmentPositions =
            new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
            {
                { "IT",        new List<string> { "Developer", "Senior Developer", "Tester", "Team Lead" } },
                { "HR",        new List<string> { "HR Executive", "HR Manager" } },
                { "Finance",   new List<string> { "Accountant", "Finance Manager" } },
                { "Marketing", new List<string> { "Marketing Executive", "Marketing Manager" } },
                { "Accounting",new List<string> { "Junior Accountant", "Senior Accountant" } }
            };

        public EmployeesController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: Employees
        public async Task<IActionResult> Index()
        {
            var employees = await _context.Employees.ToListAsync();
            return View(employees);
        }

        // GET: Employees/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var emp = await _context.Employees.FirstOrDefaultAsync(x => x.Id == id);
            if (emp == null) return NotFound();

            return View(emp);
        }

        // GET: Employees/Create
        public IActionResult Create()
        {
            var model = new Employee
            {
                EmployeeCode = GenerateNextEmployeeCode()
            };
            return View(model);
        }

        // POST: Employees/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Employee model, IFormFile ProfilePhoto)
        {
            // Default Status
            model.Status = "Active";
            ModelState.Remove("Status");
            ModelState.Remove("ConfirmPassword"); // Not mapped, ignore

            if (!ModelState.IsValid)
                return View(model);

            // Unique Email check
            if (await _context.Employees.AnyAsync(e => e.Email == model.Email))
            {
                ModelState.AddModelError("Email", "This email is already registered.");
                return View(model);
            }

            // Unique Mobile check
            if (await _context.Employees.AnyAsync(e => e.MobileNumber == model.MobileNumber))
            {
                ModelState.AddModelError("MobileNumber", "This mobile number is already registered.");
                return View(model);
            }

            // Generate EmployeeCode if missing
            if (string.IsNullOrWhiteSpace(model.EmployeeCode))
                model.EmployeeCode = GenerateNextEmployeeCode();

            // Hash Password
            model.Password = HashPassword(model.Password);

            // Save Profile Photo
            if (ProfilePhoto != null && ProfilePhoto.Length > 0)
                model.ProfileImagePath = await SaveProfilePhotoAsync(ProfilePhoto, model.EmployeeCode);

            _context.Employees.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // GET: Employees/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var emp = await _context.Employees.FindAsync(id);
            if (emp == null) return NotFound();

            // don't send password hash to UI
            emp.Password = string.Empty;
            emp.ConfirmPassword = string.Empty;

            return View(emp);
        }

        // POST: Employees/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Employee model, IFormFile ProfilePhoto)
        {
            if (id != model.Id) return NotFound();

            // ConfirmPassword is just for UI
            ModelState.Remove(nameof(Employee.ConfirmPassword));

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var emp = await _context.Employees.FindAsync(id);
            if (emp == null) return NotFound();

            // unique email/mobile excluding current
            if (await _context.Employees.AnyAsync(e => e.Email == model.Email && e.Id != id))
            {
                ModelState.AddModelError("Email", "Email is already registered.");
                return View(model);
            }

            if (await _context.Employees.AnyAsync(e => e.MobileNumber == model.MobileNumber && e.Id != id))
            {
                ModelState.AddModelError("MobileNumber", "Mobile number is already registered.");
                return View(model);
            }

            // update all fields except password & image (handled separately)
            emp.Name = model.Name;
            emp.Email = model.Email;
            emp.MobileNumber = model.MobileNumber;

            emp.Gender = model.Gender;
            emp.FatherName = model.FatherName;
            emp.MotherName = model.MotherName;
            emp.DOB_Date = model.DOB_Date;
            emp.MaritalStatus = model.MaritalStatus;

            emp.ExperienceType = model.ExperienceType;
            emp.TotalExperienceYears = model.TotalExperienceYears;
            emp.LastCompanyName = model.LastCompanyName;

            emp.JoiningDate = model.JoiningDate;
            emp.Department = model.Department;
            emp.Position = model.Position;
            emp.Salary = model.Salary;
            emp.ReportingManager = model.ReportingManager;
            emp.Address = model.Address;

            emp.HSCPercent = model.HSCPercent;
            emp.GraduationCourse = model.GraduationCourse;
            emp.GraduationPercent = model.GraduationPercent;
            emp.PostGraduationCourse = model.PostGraduationCourse;
            emp.PostGraduationPercent = model.PostGraduationPercent;

            emp.AadhaarNumber = model.AadhaarNumber;
            emp.PanNumber = model.PanNumber;

            // bank details
            emp.BankName = model.BankName;
            emp.AccountHolderName = model.AccountHolderName;
            emp.AccountNumber = model.AccountNumber;
            emp.IFSC = model.IFSC;
            emp.Branch = model.Branch;

            // update password only if new one entered
            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                emp.Password = HashPassword(model.Password);
            }

            // new profile photo?
            if (ProfilePhoto != null && ProfilePhoto.Length > 0)
            {
                emp.ProfileImagePath = await SaveProfilePhotoAsync(ProfilePhoto, emp.EmployeeCode);
            }

            _context.Update(emp);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Employees/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var emp = await _context.Employees.FirstOrDefaultAsync(x => x.Id == id);
            if (emp == null) return NotFound();

            return View(emp);
        }

        // POST: Employees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var emp = await _context.Employees.FindAsync(id);
            if (emp != null)
            {
                _context.Employees.Remove(emp);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // AJAX: department → positions
        [HttpGet]
        public IActionResult GetPositions(string department)
        {
            if (string.IsNullOrWhiteSpace(department) ||
                !DepartmentPositions.TryGetValue(department, out var positions))
            {
                return Json(new List<string>());
            }

            return Json(positions);
        }


        // ========== Helpers ==========

        private string GenerateNextEmployeeCode()
        {
            var lastCode = _context.Employees
                .OrderByDescending(e => e.EmployeeCode)
                .Select(e => e.EmployeeCode)
                .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(lastCode))
                return "IA0001";

            var prefix = lastCode.Substring(0, 2);
            var numericPart = lastCode.Substring(2);

            if (!int.TryParse(numericPart, out var number))
                return "IA0001";

            number++;
            return $"{prefix}{number:0000}";
        }

        // returns only filename (e.g. "IA0001.jpg")
        private async Task<string> SaveProfilePhotoAsync(IFormFile file, string employeeCode)
        {
            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "profiles");
            Directory.CreateDirectory(uploadsFolder);

            var ext = Path.GetExtension(file.FileName);
            var fileName = $"{employeeCode}{ext}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return fileName; // ONLY filename stored in DB
        }

        private string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return password;

            using (var sha = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        public IActionResult Gurukul()
        {
            var empId = HttpContext.Session.GetInt32("EmployeeId");

            if (empId == null)
                return RedirectToAction("Login", "Account");

            var assignedVideos =
                from v in _context.GurukulVideos
                join p in _context.GurukulProgress
                    on v.Id equals p.VideoId into gj
                from sub in gj.Where(x => x.EmployeeId == empId).DefaultIfEmpty()
                select new
                {
                    v.Title,
                    v.Category,
                    v.VideoUrl,
                    IsCompleted = sub != null && sub.IsCompleted,
                    CompletedOn = sub != null ? sub.CompletedOn : null
                };

            return View(assignedVideos.ToList());
        }

        public IActionResult ExportExcel()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var employees = _context.Employees.ToList();

            using (var package = new ExcelPackage())
            {
                var ws = package.Workbook.Worksheets.Add("Employees");

                ws.Cells[1, 1].Value = "Employee Code";
                ws.Cells[1, 2].Value = "Name";
                ws.Cells[1, 3].Value = "Email";
                ws.Cells[1, 4].Value = "Department";
                ws.Cells[1, 5].Value = "Position";

                int row = 2;

                foreach (var e in employees)
                {
                    ws.Cells[row, 1].Value = e.EmployeeCode;
                    ws.Cells[row, 2].Value = e.Name;
                    ws.Cells[row, 3].Value = e.Email;
                    ws.Cells[row, 4].Value = e.Department;
                    ws.Cells[row, 5].Value = e.Position;
                    row++;
                }

                ws.Cells.AutoFitColumns();

                var fileBytes = package.GetAsByteArray();

                return File(
                    fileBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "Employees.xlsx"
                );
            }
        }



    }
}
