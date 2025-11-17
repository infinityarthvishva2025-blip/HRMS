using HRMS.Data;
using HRMS.Models;
using HRMS.Models.ViewModels;
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

        // LIST
        public async Task<IActionResult> Index()
        {
            return View(await _context.Employees.ToListAsync());
        }

        // DETAILS
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var emp = await _context.Employees.FirstOrDefaultAsync(x => x.Id == id);
            if (emp == null) return NotFound();

            return View(emp);
        }

        // CREATE GET
        public IActionResult Create()
        {
            return View(new Employee { EmployeeCode = GenerateNextEmployeeCode() });
        }

        // CREATE POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Employee model, IFormFile ProfilePhoto)
        {
            ModelState.Remove("ConfirmPassword");

            if (!ModelState.IsValid)
                return View(model);

            // Unique Email
            if (await _context.Employees.AnyAsync(e => e.Email == model.Email))
            {
                ModelState.AddModelError("Email", "This email is already registered.");
                return View(model);
            }

            // Unique Mobile
            if (await _context.Employees.AnyAsync(e => e.MobileNumber == model.MobileNumber))
            {
                ModelState.AddModelError("MobileNumber", "This mobile number is already registered.");
                return View(model);
            }

            model.EmployeeCode ??= GenerateNextEmployeeCode();
            model.Password = HashPassword(model.Password);
            model.Status = "Active";

            if (ProfilePhoto != null)
                model.ProfileImagePath = await SaveProfilePhotoAsync(ProfilePhoto, model.EmployeeCode);

            _context.Employees.Add(model);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // EDIT GET
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var emp = await _context.Employees.FindAsync(id);
            if (emp == null) return NotFound();

            emp.Password = "";
            emp.ConfirmPassword = "";

            return View(emp);
        }

        // EDIT POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Employee model, IFormFile ProfilePhoto)
        {
            if (id != model.Id) return NotFound();

            ModelState.Remove("ConfirmPassword");

            if (!ModelState.IsValid)
                return View(model);

            var emp = await _context.Employees.FindAsync(id);
            if (emp == null) return NotFound();

            // *** FIXED: No duplicate email/mobile for same employee ***
            if (await _context.Employees.AnyAsync(e => e.Email == model.Email && e.Id != model.Id))
            {
                ModelState.AddModelError("Email", "This email is already used by another employee.");
                return View(model);
            }

            if (await _context.Employees.AnyAsync(e => e.MobileNumber == model.MobileNumber && e.Id != model.Id))
            {
                ModelState.AddModelError("MobileNumber", "This mobile number is already used by another employee.");
                return View(model);
            }

            // UPDATE FIELDS
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

            emp.BankName = model.BankName;
            emp.AccountHolderName = model.AccountHolderName;
            emp.AccountNumber = model.AccountNumber;
            emp.IFSC = model.IFSC;
            emp.Branch = model.Branch;

            // Password only if changed
            if (!string.IsNullOrWhiteSpace(model.Password))
                emp.Password = HashPassword(model.Password);

            // Photo
            if (ProfilePhoto != null)
                emp.ProfileImagePath = await SaveProfilePhotoAsync(ProfilePhoto, emp.EmployeeCode);

            _context.Update(emp);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // DELETE GET
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var emp = await _context.Employees.FirstOrDefaultAsync(x => x.Id == id);
            return emp == null ? NotFound() : View(emp);
        }

        // DELETE POST
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

        // AJAX Positions
        public IActionResult GetPositions(string department)
        {
            if (!DepartmentPositions.TryGetValue(department, out var positions))
                return Json(new List<string>());
            return Json(positions);
        }

        // Helpers
        private string GenerateNextEmployeeCode()
        {
            var last = _context.Employees
                .OrderByDescending(e => e.EmployeeCode)
                .Select(e => e.EmployeeCode)
                .FirstOrDefault();

            if (last == null)
                return "IA0001";

            int num = int.Parse(last.Substring(2)) + 1;
            return $"IA{num:0000}";
        }

        private async Task<string> SaveProfilePhotoAsync(IFormFile file, string code)
        {
            var folder = Path.Combine(_env.WebRootPath, "uploads/profiles");
            Directory.CreateDirectory(folder);

            var ext = Path.GetExtension(file.FileName);
            var fileName = code + ext;
            var path = Path.Combine(folder, fileName);

            using var stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream);

            return fileName;
        }

        private string HashPassword(string pwd)
        {
            using var sha = SHA256.Create();
            return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(pwd)));
        }
        // EMPLOYEE DASHBOARD
        public IActionResult Dashboard()
        {
            int? empId = HttpContext.Session.GetInt32("EmployeeId");

            if (empId == null)
                return RedirectToAction("Login", "Account");

            var emp = _context.Employees.FirstOrDefault(e => e.Id == empId);

            var model = new EmployeeDashboardViewModel
            {
                EmployeeName = emp?.Name,
                Department = emp?.Department,
                Position = emp?.Position,
                ProfileImage = emp?.ProfileImagePath
            };

            return View(model);
        }


        public async Task<IActionResult> MyProfile()
        {
            int? empId = HttpContext.Session.GetInt32("EmployeeId");

            if (empId == null)
                return RedirectToAction("Login", "Account");

            var emp = await _context.Employees.FindAsync(empId);

            if (emp == null)
                return NotFound();

            // Do not show hashed password
            emp.Password = "";
            emp.ConfirmPassword = "";

            return View(emp);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MyProfile(Employee model, IFormFile? ProfilePhoto)
        {
            int? empId = HttpContext.Session.GetInt32("EmployeeId");

            if (empId == null)
                return RedirectToAction("Login", "Account");

            var emp = await _context.Employees.FindAsync(empId);

            if (emp == null)
                return NotFound();

            ModelState.Remove("ConfirmPassword");

            if (!ModelState.IsValid)
                return View(model);

            // UNIQUE EMAIL VALIDATION
            if (await _context.Employees.AnyAsync(e => e.Email == model.Email && e.Id != emp.Id))
            {
                ModelState.AddModelError("Email", "Email already used by another employee.");
                return View(model);
            }

            // UNIQUE MOBILE VALIDATION
            if (await _context.Employees.AnyAsync(e => e.MobileNumber == model.MobileNumber && e.Id != emp.Id))
            {
                ModelState.AddModelError("MobileNumber", "Mobile number already used by another employee.");
                return View(model);
            }

            // UPDATE BASIC INFO
            emp.Name = model.Name;
            emp.Email = model.Email;
            emp.MobileNumber = model.MobileNumber;
            emp.Address = model.Address;
            emp.Gender = model.Gender;
            emp.MaritalStatus = model.MaritalStatus;
            emp.DOB_Date = model.DOB_Date;

            // UPDATE JOB DETAILS
            emp.Department = model.Department;
            emp.Position = model.Position;

            // UPDATE PASSWORD IF ENTERED
            if (!string.IsNullOrWhiteSpace(model.Password))
                emp.Password = HashPassword(model.Password);

            // UPDATE PROFILE PHOTO IF SELECTED
            if (ProfilePhoto != null)
                emp.ProfileImagePath = await SaveProfilePhotoAsync(ProfilePhoto, emp.EmployeeCode);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Profile updated successfully!";
            return RedirectToAction("MyProfile");
        }

    }
}
