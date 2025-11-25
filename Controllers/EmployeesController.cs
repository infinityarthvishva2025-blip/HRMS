using HRMS.Data;
using HRMS.Models;
using HRMS.Models.ViewModels;
using HRMS.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
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
            { "Accounting",new List<string> { "Junior Accountant", "Senior Accountant" } },
            { "Ganeral Manager",new List<string> { "Ganeral Manager", "Senior Ganeral Manager" } }
        };

        public EmployeesController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // ================================
        // INDEX
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

        // ================================
        // CREATE (GET)
        // ================================
        public IActionResult Create()
        {
            return View(new Employee { EmployeeCode = GenerateNextEmployeeCode() });
        }

        // ================================
        // CREATE (POST)
        // Includes 10th/12th/Grad/PG Marksheets
        // ================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
     Employee model,
     IFormFile ProfilePhoto,
     IFormFile AadhaarFile,
     IFormFile PanFile,
     IFormFile PassbookFile,
     List<IFormFile> ExperienceCertificateFiles,
     IFormFile MedicalDocumentFile,
     IFormFile TenthMarksheetFile,
     IFormFile TwelfthMarksheetFile,
     IFormFile GraduationMarksheetFile,
     IFormFile PostGraduationMarksheetFile
 )
        {
            // REMOVE NON-NEEDED MODEL VALIDATION
            ModelState.Remove("ConfirmPassword");
            ModelState.Remove("MedicalDocumentFile");
            ModelState.Remove("MarksheetFile");

            // CONDITIONAL VALIDATION
            if (model.HasDisease == "Yes" && MedicalDocumentFile == null)
            {
                ModelState.AddModelError("MedicalDocumentFile", "Please upload medical document because you selected 'Yes'.");
            }

            if (!ModelState.IsValid)
                return View(model);

            // UNIQUE MOBILE / EMAIL CHECKS
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

            // SET DEFAULTS
            model.EmployeeCode ??= GenerateNextEmployeeCode();
            model.Status = "Active";

            // CREATE FOLDER
            string empFolder = Path.Combine(_env.WebRootPath, "uploads/employees", model.EmployeeCode);
            Directory.CreateDirectory(empFolder);

            // STORE REQUIRED FILES
            model.ProfileImagePath = await SaveEmployeeFile(ProfilePhoto, empFolder, "profile");
            model.AadhaarFilePath = await SaveEmployeeFile(AadhaarFile, empFolder, "aadhaar");
            model.PanFilePath = await SaveEmployeeFile(PanFile, empFolder, "pan");
            model.PassbookFilePath = await SaveEmployeeFile(PassbookFile, empFolder, "passbook");

            // MULTIPLE EXPERIENCE FILES
            if (ExperienceCertificateFiles != null && ExperienceCertificateFiles.Any())
            {
                List<string> savedExp = new();

                foreach (var file in ExperienceCertificateFiles)
                {
                    var saved = await SaveEmployeeFile(file, empFolder, $"exp_{Guid.NewGuid()}");
                    savedExp.Add(saved);
                }

                model.ExperienceCertificateFilePath = string.Join(",", savedExp);
            }

            // MEDICAL (OPTIONAL)
            model.MedicalDocumentFilePath =
                await SaveEmployeeFile(MedicalDocumentFile, empFolder, "medical");

            // EDUCATION FILES
            model.TenthMarksheetFilePath =
                await SaveEmployeeFile(TenthMarksheetFile, empFolder, "10th");

            model.TwelfthMarksheetFilePath =
                await SaveEmployeeFile(TwelfthMarksheetFile, empFolder, "12th");

            model.GraduationMarksheetFilePath =
                await SaveEmployeeFile(GraduationMarksheetFile, empFolder, "graduation");

            model.PostGraduationMarksheetFilePath =
                await SaveEmployeeFile(PostGraduationMarksheetFile, empFolder, "pg");

            // SAVE
            _context.Employees.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }


        // ================================
        // EDIT (GET)
        // ================================
        public IActionResult Edit(int id)
        {
            var emp = _context.Employees.Find(id);
            if (emp == null) return NotFound();

            var vm = new EmployeeEditVm
            {
                Id = emp.Id,
                Name = emp.Name,
                Email = emp.Email,
                MobileNumber = emp.MobileNumber,
                Department = emp.Department,
                Position = emp.Position,
                Role = emp.Role,
                Salary = emp.Salary,
                AccountHolderName = emp.AccountHolderName,
                BankName = emp.BankName,
                AccountNumber = emp.AccountNumber,
                IFSC = emp.IFSC,
                Branch = emp.Branch,
                EmergencyContactName = emp.EmergencyContactName,
                EmergencyContactRelationship = emp.EmergencyContactRelationship,
                EmergencyContactMobile = emp.EmergencyContactMobile,
                EmergencyContactAddress = emp.EmergencyContactAddress
            };

            return View(vm);
        }



        // ================================
        // EDIT (POST)
        // Supports updating new education docs
        // ================================
        [HttpPost]
        public async Task<IActionResult> Edit(EmployeeEditVm model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var emp = _context.Employees.Find(model.Id);
            if (emp == null)
                return NotFound();

            // Update only provided fields
            emp.Name = model.Name ?? emp.Name;
            emp.Email = model.Email ?? emp.Email;
            emp.MobileNumber = model.MobileNumber ?? emp.MobileNumber;
            emp.AlternateMobileNumber = model.AlternateMobileNumber ?? emp.AlternateMobileNumber;
            emp.Department = model.Department ?? emp.Department;
            emp.Position = model.Position ?? emp.Position;
            emp.Role = model.Role ?? emp.Role;
            emp.Salary = model.Salary ?? emp.Salary;
            emp.BankName = model.BankName ?? emp.BankName;
            emp.AccountHolderName = model.AccountHolderName ?? emp.AccountHolderName;
            emp.AccountNumber = model.AccountNumber ?? emp.AccountNumber;
            emp.IFSC = model.IFSC ?? emp.IFSC;
            emp.Branch = model.Branch ?? emp.Branch;
            emp.EmergencyContactName = model.EmergencyContactName ?? emp.EmergencyContactName;
            emp.EmergencyContactRelationship = model.EmergencyContactRelationship ?? emp.EmergencyContactRelationship;
            emp.EmergencyContactMobile = model.EmergencyContactMobile ?? emp.EmergencyContactMobile;
            emp.EmergencyContactAddress = model.EmergencyContactAddress ?? emp.EmergencyContactAddress;

            // Update password only if entered
            if (!string.IsNullOrEmpty(model.Password))
                emp.Password = HashPassword(model.Password);

            // Handle file uploads (optional)
            if (model.ProfilePhoto != null)
                emp.ProfileImagePath = await SaveEmployeeFile(model.ProfilePhoto, emp.EmployeeCode, "ProfilePhoto", emp.ProfileImagePath);

            if (model.PanFile != null)
                emp.PanFilePath = await SaveEmployeeFile(model.PanFile, emp.EmployeeCode, "PanFile", emp.PanFilePath);

            // Save to DB
            await _context.SaveChangesAsync();
            TempData["success"] = "Employee details updated successfully!";
            return RedirectToAction("Index");
        }

        private async Task<string> SaveEmployeeFile(IFormFile file, string empCode, string name, string existing = null)
        {
            if (file == null) return existing;

            var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "employees", empCode);
            if (!Directory.Exists(uploadDir))
                Directory.CreateDirectory(uploadDir);

            var fileName = $"{name}_{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadDir, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return fileName;
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
                return "IA00001";

            int num = int.Parse(last.Substring(2)) + 1;
            return $"IA{num:0000}";
        }

        // ==========================================================
        // GET POSITIONS
        // ==========================================================
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

        public async Task<IActionResult> Dashboard()
        {
            var empId = HttpContext.Session.GetInt32("EmployeeId");

            if (empId == null || empId == 0)
                return RedirectToAction("Login", "Account");

            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == empId);
            if (employee == null)
                return RedirectToAction("Login", "Account");

            string empCode = employee.EmployeeCode;

            // Total attendance records
            int totalAttendance = await _context.Attendances
                .CountAsync(a => a.Emp_Code == empCode);

            // Today’s attendance
            //var todayRecord = await _context.Attendances
            //    .FirstOrDefaultAsync(a =>
            //        a.Emp_Code == empCode &&
            //        a.Date == DateTime.Today
            //    );

           // string todayStatus = todayRecord != null ? "Present" : "Not Marked";

            // Total leaves
            int totalLeaves = await _context.Leaves
                .CountAsync(l => l.EmployeeId == empId);

            // Upcoming birthdays (next 30 days)
            var upcomingBirthdays = await _context.Employees
                .Where(e =>
                    e.DOB_Date.HasValue &&
                    e.DOB_Date.Value.Month == DateTime.Today.Month &&
                    e.DOB_Date.Value.Day >= DateTime.Today.Day)
                .OrderBy(e => e.DOB_Date.Value.Day)
                .Take(5)
                .ToListAsync();

            var vm = new EmployeeDashboardViewModel
            {
                Employee = employee,
                TotalAttendance = totalAttendance,
               // TodayStatus = todayStatus,
                TotalLeave = totalLeaves,
                UpcomingBirthdays = upcomingBirthdays
            };

            return View(vm);
        }


        public async Task<IActionResult> MyProfile()
        {
            // 1️⃣ Check Employee Session
            var empId = HttpContext.Session.GetInt32("EmployeeId");

            if (empId == null || empId == 0)
            {
                return RedirectToAction("Login", "Account");  // Not logged in
            }

            // 2️⃣ Fetch the employee details
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Id == empId);

            if (employee == null)
            {
                return RedirectToAction("Login", "Account"); // Employee not found
            }

            // 3️⃣ Return the profile view
            return View(employee);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == id);
            if (employee == null) return NotFound();

            return View(employee);   // You need Delete.cshtml view
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == id);
            if (employee == null)
                return NotFound();

            // EMPLOYEE FOLDER PATH
            string empFolder = Path.Combine(_env.WebRootPath, "uploads/employees", employee.EmployeeCode);

            // DELETE EMPLOYEE FROM DATABASE
            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();

            // DELETE EMPLOYEE FOLDER AND FILES
            if (Directory.Exists(empFolder))
            {
                Directory.Delete(empFolder, true);   // true = delete all files & subfolders
            }

            return RedirectToAction(nameof(Index));
        }



public IActionResult ExportExcel()
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        var employees = _context.Employees.ToList();

        using (var package = new ExcelPackage())
        {
            var ws = package.Workbook.Worksheets.Add("Employees");

            // HEADER
            ws.Cells[1, 1].Value = "Employee Code";
            ws.Cells[1, 2].Value = "Name";
            ws.Cells[1, 3].Value = "Email";
            ws.Cells[1, 4].Value = "Department";
            ws.Cells[1, 5].Value = "Position";

            int row = 2;

            foreach (var emp in employees)
            {
                ws.Cells[row, 1].Value = emp.EmployeeCode;
                ws.Cells[row, 2].Value = emp.Name;
                ws.Cells[row, 3].Value = emp.Email;
                ws.Cells[row, 4].Value = emp.Department;
                ws.Cells[row, 5].Value = emp.Position;

                row++;
            }

            var fileBytes = package.GetAsByteArray();
            return File(fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Employees.xlsx");
        }
    }

        [Authorize]
        public IActionResult MySalarySlip()
        {
            var employee = _context.Employees
                .FirstOrDefault(e => e.Email == User.Identity.Name);

            if (employee == null) return NotFound();

            // Fetch the salary slip PDF path or generate it
            var slipPath = $"~/uploads/salaryslips/{employee.EmployeeCode}.pdf";
            if (!System.IO.File.Exists(Path.Combine(_env.WebRootPath, "uploads/salaryslips", $"{employee.EmployeeCode}.pdf")))
                return View("NoSlip");

            return File(slipPath, "application/pdf");
        }

        public IActionResult DownloadSalarySlip(int month, int year)
        {
            var emp = _context.Employees.FirstOrDefault(e => e.Email == User.Identity.Name);
            if (emp == null) return NotFound();

            var filePath = Path.Combine(_env.WebRootPath, "SalarySlips", $"{emp.EmployeeCode}_{month}_{year}.pdf");
            if (!System.IO.File.Exists(filePath))
                return Content("Salary slip not found for selected month and year.");

            return PhysicalFile(filePath, "application/pdf", $"{emp.EmployeeCode}_{month}_{year}.pdf");
        }


    }
}
