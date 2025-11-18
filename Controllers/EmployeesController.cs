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

        // ================================
        // LIST
        // ================================
        public async Task<IActionResult> Index()
        {
            return View(await _context.Employees.ToListAsync());
        }

        // ================================
        // DETAILS
        // ================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var emp = await _context.Employees.FirstOrDefaultAsync(x => x.Id == id);
            if (emp == null) return NotFound();

            return View(emp);
        }

        // ========================================
        // CREATE EMPLOYEE (HR ONLY)
        // ========================================
        public IActionResult Create()
        {
            return View(new Employee { EmployeeCode = GenerateNextEmployeeCode() });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Employee model,
            IFormFile ProfilePhoto,
            IFormFile AadhaarFile,
            IFormFile PanFile,
            IFormFile PassbookFile,
            IFormFile MarksheetFile)
        {
            ModelState.Remove("ConfirmPassword");

            if (!ModelState.IsValid)
                return View(model);

            // Unique Email / Mobile
            if (await _context.Employees.AnyAsync(e => e.Email == model.Email))
            {
                ModelState.AddModelError("Email", "This email is already registered.");
                return View(model);
            }

            if (await _context.Employees.AnyAsync(e => e.MobileNumber == model.MobileNumber))
            {
                ModelState.AddModelError("MobileNumber", "This mobile number is already registered.");
                return View(model);
            }

            model.EmployeeCode ??= GenerateNextEmployeeCode();
            model.Status = "Active";

            // ❌ DO NOT HASH PASSWORD (your requirement)
            // model.Password = HashPassword(model.Password);

            // Create Employee Folder
            string empFolder = Path.Combine(_env.WebRootPath, "uploads/employees", model.EmployeeCode);
            Directory.CreateDirectory(empFolder);

            // Save all files
            model.ProfileImagePath = await SaveEmployeeFile(ProfilePhoto, empFolder, "profile");
            model.AadhaarFilePath = await SaveEmployeeFile(AadhaarFile, empFolder, "aadhaar");
            model.PanFilePath = await SaveEmployeeFile(PanFile, empFolder, "pan");
            model.PassbookFilePath = await SaveEmployeeFile(PassbookFile, empFolder, "passbook");
            model.MarksheetFilePath = await SaveEmployeeFile(MarksheetFile, empFolder, "marksheet");

            _context.Employees.Add(model);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // ========================================
        // EDIT (HR ONLY)
        // ========================================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var emp = await _context.Employees.FindAsync(id);
            if (emp == null) return NotFound();

            emp.Password = "";
            emp.ConfirmPassword = "";

            return View(emp);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Employee model,
            IFormFile ProfilePhoto,
            IFormFile AadhaarFile,
            IFormFile PanFile,
            IFormFile PassbookFile,
            IFormFile MarksheetFile)
        {
            if (id != model.Id) return NotFound();

            ModelState.Remove("ConfirmPassword");

            if (!ModelState.IsValid)
                return View(model);

            var emp = await _context.Employees.FindAsync(id);
            if (emp == null) return NotFound();

            // Unique checks
            if (await _context.Employees.AnyAsync(e => e.Email == model.Email && e.Id != emp.Id))
            {
                ModelState.AddModelError("Email", "This email is already used by another employee.");
                return View(model);
            }

            if (await _context.Employees.AnyAsync(e => e.MobileNumber == model.MobileNumber && e.Id != emp.Id))
            {
                ModelState.AddModelError("MobileNumber", "This mobile number is already used by another employee.");
                return View(model);
            }

            // Update Main Fields
            emp.Name = model.Name;
            emp.Email = model.Email;
            emp.MobileNumber = model.MobileNumber;
            emp.Address = model.Address;

            emp.Department = model.Department;
            emp.Position = model.Position;
            emp.ReportingManager = model.ReportingManager;
            emp.Salary = model.Salary;

            emp.Gender = model.Gender;
            emp.FatherName = model.FatherName;
            emp.MotherName = model.MotherName;
            emp.MaritalStatus = model.MaritalStatus;
            emp.DOB_Date = model.DOB_Date;

            emp.AadhaarNumber = model.AadhaarNumber;
            emp.PanNumber = model.PanNumber;

            // Update password if changed (you can remove hashing if needed)
            if (!string.IsNullOrWhiteSpace(model.Password))
                emp.Password = HashPassword(model.Password);

            // Save updated documents
            string empFolder = Path.Combine(_env.WebRootPath, "uploads/employees", emp.EmployeeCode);
            Directory.CreateDirectory(empFolder);

            emp.ProfileImagePath = await SaveEmployeeFile(ProfilePhoto, empFolder, "profile", emp.ProfileImagePath);
            emp.AadhaarFilePath = await SaveEmployeeFile(AadhaarFile, empFolder, "aadhaar", emp.AadhaarFilePath);
            emp.PanFilePath = await SaveEmployeeFile(PanFile, empFolder, "pan", emp.PanFilePath);
            emp.PassbookFilePath = await SaveEmployeeFile(PassbookFile, empFolder, "passbook", emp.PassbookFilePath);
            emp.MarksheetFilePath = await SaveEmployeeFile(MarksheetFile, empFolder, "marksheet", emp.MarksheetFilePath);

            _context.Update(emp);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ==========================================================
        // EMPLOYEE DASHBOARD
        // ==========================================================
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

        // ==========================================================
        // EMPLOYEE — VIEW ONLY PROFILE
        // ==========================================================
        public async Task<IActionResult> MyProfile()
        {
            int? empId = HttpContext.Session.GetInt32("EmployeeId");
            if (empId == null)
                return RedirectToAction("Login", "Account");

            var emp = await _context.Employees.FindAsync(empId);
            if (emp == null)
                return NotFound();

            return View(emp);
        }

        // ==========================================================
        // FILE SAVE METHOD
        // ==========================================================
        private async Task<string> SaveEmployeeFile(IFormFile file, string folder, string name, string existingFile = null)
        {
            if (file == null) return existingFile;

            string ext = Path.GetExtension(file.FileName);
            string newFile = $"{name}{ext}";
            string filePath = Path.Combine(folder, newFile);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return newFile;
        }

        // ==========================================================
        // PASSWORD HASH
        // ==========================================================
        private string HashPassword(string pwd)
        {
            using var sha = SHA256.Create();
            return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(pwd)));
        }

        // ==========================================================
        // AUTO EMPLOYEE CODE
        // ==========================================================
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

        [HttpGet]
        public IActionResult GetPositions(string department)
        {
            var positions = new List<string>();

            if (!string.IsNullOrEmpty(department))
            {
                if (DepartmentPositions.ContainsKey(department))
                {
                    positions = DepartmentPositions[department];
                }
            }

            return Json(positions);
        }


    }
}
