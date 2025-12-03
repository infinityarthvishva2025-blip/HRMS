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
            { "IT",        new List<string> { "Software Developer", "Senior Developer", "IT Admin", "Team Lead" } },
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
                EmployeeCode = emp.EmployeeCode,
                Name = emp.Name,
                Email = emp.Email,
                MobileNumber = emp.MobileNumber,
                AlternateMobileNumber = emp.AlternateMobileNumber,
                Gender = emp.Gender,
                FatherName = emp.FatherName,
                MotherName = emp.MotherName,
                DOB_Date = emp.DOB_Date,
                MaritalStatus = emp.MaritalStatus,
                Address = emp.Address,
                PermanentAddress = emp.PermanentAddress,
                ExperienceType = emp.ExperienceType,
                TotalExperienceYears = emp.TotalExperienceYears,
                LastCompanyName = emp.LastCompanyName,
                HasDisease = emp.HasDisease,
                DiseaseName = emp.DiseaseName,
                DiseaseSince = emp.DiseaseSince,
                MedicinesRequired = emp.MedicinesRequired,
                DoctorName = emp.DoctorName,
                DoctorContact = emp.DoctorContact,
               
                JoiningDate = emp.JoiningDate,
                Department = emp.Department,
                Position = emp.Position,
                Role = emp.Role,
                Salary = emp.Salary,
                HSCPercent = emp.HSCPercent,
                GraduationCourse = emp.GraduationCourse,
                GraduationPercent = emp.GraduationPercent,
                PostGraduationCourse = emp.PostGraduationCourse,
                PostGraduationPercent = emp.PostGraduationPercent,
                AadhaarNumber = emp.AadhaarNumber,
                PanNumber = emp.PanNumber,
                AccountHolderName = emp.AccountHolderName,
                BankName = emp.BankName,
                AccountNumber = emp.AccountNumber,
                IFSC = emp.IFSC,
                Branch = emp.Branch,
                EmergencyContactName = emp.EmergencyContactName,
                EmergencyContactRelationship = emp.EmergencyContactRelationship,
                EmergencyContactMobile = emp.EmergencyContactMobile,
                EmergencyContactAddress = emp.EmergencyContactAddress,

                ProfileImagePath = emp.ProfileImagePath,
                ExperienceCertificateFilePath = emp.ExperienceCertificateFilePath,
                MedicalDocumentFilePath = emp.MedicalDocumentFilePath,
                TenthMarksheetFilePath = emp.TenthMarksheetFilePath,
                TwelfthMarksheetFilePath = emp.TwelfthMarksheetFilePath,
                GraduationMarksheetFilePath = emp.GraduationMarksheetFilePath,
                PostGraduationMarksheetFilePath = emp.PostGraduationMarksheetFilePath,
                AadhaarFilePath = emp.AadhaarFilePath,
                PanFilePath = emp.PanFilePath,
                PassbookFilePath = emp.PassbookFilePath
            };
            // DISTINCT departments from Employees table
            // Load distinct departments from Employees table
            ViewBag.Departments = _context.Employees
                .Where(e => !string.IsNullOrEmpty(e.Department))
                .Select(e => e.Department.Trim())
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            return View(vm);

        }



        // ================================
        // EDIT (POST)
        // ================================
        [HttpPost]
        public async Task<IActionResult> Edit(EmployeeEditVm model)
        {
            var emp = _context.Employees.FirstOrDefault(x => x.Id == model.Id);
            if (emp == null)
                return NotFound();

            // BASIC
            emp.Name = model.Name ?? emp.Name;
            emp.Email = model.Email ?? emp.Email;
            emp.MobileNumber = model.MobileNumber ?? emp.MobileNumber;
            emp.AlternateMobileNumber = model.AlternateMobileNumber ?? emp.AlternateMobileNumber;

            // PASSWORD UPDATE ONLY WHEN ENTERED
            // PASSWORD LOGIC
            // Only validate password if user entered a new one
            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                if (model.Password != model.ConfirmPassword)
                {
                    ModelState.AddModelError("ConfirmPassword", "Passwords do not match.");
                    return View(model);
                }

                emp.Password = model.Password;
            }




            // PERSONAL
            emp.Gender = model.Gender ?? emp.Gender;
            emp.FatherName = model.FatherName ?? emp.FatherName;
            emp.MotherName = model.MotherName ?? emp.MotherName;
            emp.DOB_Date = model.DOB_Date ?? emp.DOB_Date;
            emp.MaritalStatus = model.MaritalStatus ?? emp.MaritalStatus;
            emp.Address = model.Address ?? emp.Address;
            emp.PermanentAddress = model.PermanentAddress ?? emp.PermanentAddress;

            // EXPERIENCE
            emp.ExperienceType = model.ExperienceType ?? emp.ExperienceType;
            emp.TotalExperienceYears = model.TotalExperienceYears ?? emp.TotalExperienceYears;
            emp.LastCompanyName = model.LastCompanyName ?? emp.LastCompanyName;

            // HEALTH
            emp.HasDisease = model.HasDisease ?? emp.HasDisease;
            emp.DiseaseName = model.DiseaseName ?? emp.DiseaseName;
            emp.DiseaseSince = model.DiseaseSince ?? emp.DiseaseSince;
            emp.MedicinesRequired = model.MedicinesRequired ?? emp.MedicinesRequired;
            emp.DoctorName = model.DoctorName ?? emp.DoctorName;
            emp.DoctorContact = model.DoctorContact ?? emp.DoctorContact;
            

            // JOB
            emp.JoiningDate = model.JoiningDate ?? emp.JoiningDate;
            emp.Department = model.Department ?? emp.Department;
            emp.Position = model.Position ?? emp.Position;
            emp.Role = model.Role ?? emp.Role;
            emp.Salary = model.Salary ?? emp.Salary;

            // EDUCATION
            emp.HSCPercent = model.HSCPercent ?? emp.HSCPercent;
            emp.GraduationCourse = model.GraduationCourse ?? emp.GraduationCourse;
            emp.GraduationPercent = model.GraduationPercent ?? emp.GraduationPercent;
            emp.PostGraduationCourse = model.PostGraduationCourse ?? emp.PostGraduationCourse;
            emp.PostGraduationPercent = model.PostGraduationPercent ?? emp.PostGraduationPercent;

            // IDs
            emp.AadhaarNumber = model.AadhaarNumber ?? emp.AadhaarNumber;
            emp.PanNumber = model.PanNumber ?? emp.PanNumber;

            // BANK
            emp.AccountHolderName = model.AccountHolderName ?? emp.AccountHolderName;
            emp.BankName = model.BankName ?? emp.BankName;
            emp.AccountNumber = model.AccountNumber ?? emp.AccountNumber;
            emp.IFSC = model.IFSC ?? emp.IFSC;
            emp.Branch = model.Branch ?? emp.Branch;
            // Emergency Contact
            emp.EmergencyContactName = model.EmergencyContactName ?? emp.EmergencyContactName;
            emp.EmergencyContactRelationship = model.EmergencyContactRelationship ?? emp.EmergencyContactRelationship;
            emp.EmergencyContactMobile = model.EmergencyContactMobile ?? emp.EmergencyContactMobile;
            emp.EmergencyContactAddress = model.EmergencyContactAddress ?? emp.EmergencyContactAddress;


            // FILE UPLOADS
            emp.ProfileImagePath = await SaveEmployeeFile(model.ProfilePhoto, emp.EmployeeCode, "Profile", emp.ProfileImagePath);
            emp.AadhaarFilePath = await SaveEmployeeFile(model.AadhaarFile, emp.EmployeeCode, "Aadhaar", emp.AadhaarFilePath);
            emp.PanFilePath = await SaveEmployeeFile(model.PanFile, emp.EmployeeCode, "Pan", emp.PanFilePath);
            emp.PassbookFilePath = await SaveEmployeeFile(model.PassbookFile, emp.EmployeeCode, "Passbook", emp.PassbookFilePath);
            emp.MedicalDocumentFilePath = await SaveEmployeeFile(model.MedicalDocumentFile, emp.EmployeeCode, "Medical", emp.MedicalDocumentFilePath);

            emp.TenthMarksheetFilePath = await SaveEmployeeFile(model.TenthMarksheetFile, emp.EmployeeCode, "Tenth", emp.TenthMarksheetFilePath);
            emp.TwelfthMarksheetFilePath = await SaveEmployeeFile(model.TwelfthMarksheetFile, emp.EmployeeCode, "Twelfth", emp.TwelfthMarksheetFilePath);
            emp.GraduationMarksheetFilePath = await SaveEmployeeFile(model.GraduationMarksheetFile, emp.EmployeeCode, "Graduation", emp.GraduationMarksheetFilePath);
            emp.PostGraduationMarksheetFilePath = await SaveEmployeeFile(model.PostGraduationMarksheetFile, emp.EmployeeCode, "PG", emp.PostGraduationMarksheetFilePath);

            //EMERGENCY DETAILS

           



            // MULTIPLE EXPERIENCE FILES
            if (model.ExperienceCertificateFiles != null && model.ExperienceCertificateFiles.Count > 0)
            {
                List<string> certFiles = new();

                foreach (var file in model.ExperienceCertificateFiles)
                {
                    var saved = await SaveEmployeeFile(file, emp.EmployeeCode, "Experience", null);
                    certFiles.Add(saved);
                }

                emp.ExperienceCertificateFilePath = string.Join(",", certFiles);
            }

            await _context.SaveChangesAsync();

            TempData["success"] = "Employee details updated successfully!";
            return RedirectToAction("Edit", new { id = emp.Id });
        }

        private async Task<string> SaveEmployeeFile(IFormFile file, string empCode, string name, string existing = null)
        {
            if (file == null) return existing;

            // 👉 Save to C Drive instead of wwwroot
            var uploadDir = Path.Combine("C:\\HRMSFiles", empCode);

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

        public IActionResult ViewDocument(string empCode, string fileName)
        {
            if (string.IsNullOrEmpty(empCode) || string.IsNullOrEmpty(fileName))
                return NotFound();

            string filePath = Path.Combine("C:\\HRMSFiles", empCode, fileName);

            if (!System.IO.File.Exists(filePath))
                return NotFound("File not found");

            string contentType = "application/octet-stream";

            // Detect image type for preview
            var ext = Path.GetExtension(fileName).ToLower();
            if (ext == ".jpg" || ext == ".jpeg") contentType = "image/jpeg";
            if (ext == ".png") contentType = "image/png";
            if (ext == ".pdf") contentType = "application/pdf";

            return PhysicalFile(filePath, contentType);
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
            return $"IA{num:00000}";
        }

        // ==========================================================
        // GET POSITIONS
        // ==========================================================
        // GET: /Employees/GetPositions?department=IT
        [HttpGet]
        public async Task<IActionResult> GetPositions(string department)
        {
            if (string.IsNullOrEmpty(department))
                return Json(new List<string>());

            var positions = await _context.Employees
                .Where(e =>
                    e.Department != null &&
                    e.Position != null &&
                    e.Department.ToLower() == department.ToLower())
                .Select(e => e.Position.Trim())
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();

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

            // Total leaves
            int totalLeaves = await _context.Leaves
                .CountAsync(l => l.EmployeeId == empId);

            // Upcoming birthdays
            var upcomingBirthdays = await _context.Employees
                .Where(e =>
                    e.DOB_Date.HasValue &&
                    e.DOB_Date.Value.Month == DateTime.Today.Month &&
                    e.DOB_Date.Value.Day >= DateTime.Today.Day)
                .OrderBy(e => e.DOB_Date.Value.Day)
                .Take(5)
                .ToListAsync();

            // ============================================================
            // 🔥 UNREAD ANNOUNCEMENTS COUNT FOR NOTIFICATION BELL
            // ============================================================
            var allAnnouncements = await _context.Announcements.ToListAsync();

            ViewBag.UnreadCount = allAnnouncements.Count(a =>
                (
                    a.IsGlobal ||
                    (!string.IsNullOrEmpty(a.TargetDepartments) &&
                        a.TargetDepartments.Split(',').Contains(employee.Department)) ||
                    (!string.IsNullOrEmpty(a.TargetEmployees) &&
                        a.TargetEmployees.Split(',').Contains(empId.ToString()))
                )
                &&
                (string.IsNullOrEmpty(a.ReadByEmployees) ||
                    !a.ReadByEmployees.Split(',').Contains(empId.ToString()))
            );
            // ============================================================


            var vm = new EmployeeDashboardViewModel
            {
                Employee = employee,
                TotalAttendance = totalAttendance,
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


        [HttpPost]
      
        public IActionResult UpdateStatus(int id, [FromBody] StatusUpdateModel model)
        {
            var emp = _context.Employees.FirstOrDefault(e => e.Id == id);

            if (emp == null)
                return Json(new { success = false });

            emp.Status = model.Status;

            if (model.Status == "Active")
            {
                emp.DeactiveReason = null; // CLEAR reason when active again
            }
            else
            {
                emp.DeactiveReason = model.Reason; // Save reason
            }

            _context.SaveChanges();

            return Json(new { success = true });
        }

        public class StatusUpdateModel
        {
            public string Status { get; set; }
            public string Reason { get; set; }
        }



    }
}
