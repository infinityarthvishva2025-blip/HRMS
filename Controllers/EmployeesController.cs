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
using OfficeOpenXml;

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
        public async Task<IActionResult> Edit(int id)
        {
            var employee = await _context.Employees.FindAsync(id);

            if (employee == null)
                return NotFound();

            return View(employee);
        }


        // ================================
        // EDIT (POST)
        // Supports updating new education docs
        // ================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Employee model,
     IFormFile ProfilePhoto,
     IFormFile AadhaarFile,
     IFormFile PanFile,
     IFormFile PassbookFile,
     IFormFile MedicalDocumentFile,
     IFormFile TenthMarksheetFile,
     IFormFile TwelfthMarksheetFile,
     IFormFile GraduationMarksheetFile,
     IFormFile PostGraduationMarksheetFile,
     List<IFormFile> ExperienceCertificateFiles)
        {
            if (!ModelState.IsValid)
                return View(model);

            var emp = await _context.Employees.FindAsync(id);
            if (emp == null)
                return NotFound();

            string empFolder = Path.Combine(_env.WebRootPath, "uploads/employees", emp.EmployeeCode);
            Directory.CreateDirectory(empFolder);

            // *************** FILE UPLOAD HANDLING ***************

            emp.ProfileImagePath =
                await SaveEmployeeFile(ProfilePhoto, empFolder, "profile", emp.ProfileImagePath);

            emp.AadhaarFilePath =
                await SaveEmployeeFile(AadhaarFile, empFolder, "aadhaar", emp.AadhaarFilePath);

            emp.PanFilePath =
                await SaveEmployeeFile(PanFile, empFolder, "pan", emp.PanFilePath);

            emp.PassbookFilePath =
                await SaveEmployeeFile(PassbookFile, empFolder, "passbook", emp.PassbookFilePath);

            emp.MedicalDocumentFilePath =
                await SaveEmployeeFile(MedicalDocumentFile, empFolder, "medical_document", emp.MedicalDocumentFilePath);

            emp.TenthMarksheetFilePath =
                await SaveEmployeeFile(TenthMarksheetFile, empFolder, "10th", emp.TenthMarksheetFilePath);

            emp.TwelfthMarksheetFilePath =
                await SaveEmployeeFile(TwelfthMarksheetFile, empFolder, "12th", emp.TwelfthMarksheetFilePath);

            emp.GraduationMarksheetFilePath =
                await SaveEmployeeFile(GraduationMarksheetFile, empFolder, "grad", emp.GraduationMarksheetFilePath);

            emp.PostGraduationMarksheetFilePath =
                await SaveEmployeeFile(PostGraduationMarksheetFile, empFolder, "pg", emp.PostGraduationMarksheetFilePath);

            // *************** EXPERIENCE CERTIFICATES (MULTIPLE FILES) ***************
            if (ExperienceCertificateFiles != null && ExperienceCertificateFiles.Count > 0)
            {
                List<string> uploadedFiles = new List<string>();

                foreach (var file in ExperienceCertificateFiles)
                {
                    if (file != null && file.Length > 0)
                    {
                        var ext = Path.GetExtension(file.FileName);
                        var fileName = $"exp_{Guid.NewGuid()}{ext}";
                        var filePath = Path.Combine(empFolder, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        uploadedFiles.Add(fileName);
                    }
                }

                // Append new files with existing
                if (!string.IsNullOrEmpty(emp.ExperienceCertificateFilePath))
                {
                    emp.ExperienceCertificateFilePath += "," + string.Join(",", uploadedFiles);
                }
                else
                {
                    emp.ExperienceCertificateFilePath = string.Join(",", uploadedFiles);
                }
            }



            // *************** OPTIONAL PASSWORD UPDATE ***************
            if (!string.IsNullOrEmpty(model.Password))
            {
                emp.Password = model.Password;
            }

            // *************** UPDATE ALL OTHER NORMAL FIELDS ***************
            emp.Name = model.Name;
            emp.Email = model.Email;
            emp.MobileNumber = model.MobileNumber;
            emp.AlternateMobileNumber = model.AlternateMobileNumber;
            emp.Gender = model.Gender;
            emp.FatherName = model.FatherName;
            emp.MotherName = model.MotherName;
            emp.DOB_Date = model.DOB_Date;
            emp.MaritalStatus = model.MaritalStatus;

            emp.Address = model.Address;
            emp.PermanentAddress = model.PermanentAddress;

            emp.ExperienceType = model.ExperienceType;
            emp.TotalExperienceYears = model.TotalExperienceYears;
            emp.LastCompanyName = model.LastCompanyName;

            emp.HasDisease = model.HasDisease;
            emp.DiseaseName = model.DiseaseName;
            emp.DiseaseSince = model.DiseaseSince;
            emp.MedicinesRequired = model.MedicinesRequired;
            emp.DoctorName = model.DoctorName;
            emp.DoctorContact = model.DoctorContact;
            emp.LastAffectedDate = model.LastAffectedDate;

            emp.JoiningDate = model.JoiningDate;
            emp.Department = model.Department;
            emp.Position = model.Position;
            emp.Role = model.Role;
            emp.Salary = model.Salary;

            emp.AadhaarNumber = model.AadhaarNumber;
            emp.PanNumber = model.PanNumber;

            emp.AccountHolderName = model.AccountHolderName;
            emp.BankName = model.BankName;
            emp.AccountNumber = model.AccountNumber;
            emp.IFSC = model.IFSC;
            emp.Branch = model.Branch;

            emp.EmergencyContactName = model.EmergencyContactName;
            emp.EmergencyContactRelationship = model.EmergencyContactRelationship;
            emp.EmergencyContactMobile = model.EmergencyContactMobile;
            emp.EmergencyContactAddress = model.EmergencyContactAddress;

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }


        // ==========================================================
        // FILE SAVE METHOD
        // ==========================================================
        private async Task<string> SaveEmployeeFile(IFormFile file, string folder, string name, string existing = null)
        {
            // No new file → keep old file
            if (file == null || file.Length == 0)
                return existing;

            // Ensure folder exists
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string ext = Path.GetExtension(file.FileName);
            string fileName = $"{name}{ext}";
            string filePath = Path.Combine(folder, fileName);

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


}
}
