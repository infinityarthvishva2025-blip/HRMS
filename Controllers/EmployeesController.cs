using DocumentFormat.OpenXml.Bibliography;
using HRMS.Data;
using HRMS.Helpers;
using HRMS.Models;
using HRMS.Models.ViewModels;
using HRMS.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.RenderTree;
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
using System.Threading;
using System.Threading.Tasks;


namespace HRMS.Controllers
{
   
    public class EmployeesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private const string FILE_ROOT = @"C:\HRMSFiles";

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


        public async Task<IActionResult> Details(int? id, string token)
        {
            int empId;

            if (!string.IsNullOrEmpty(token))
            {
                if (!UrlEncryptionHelper.TryDecryptToken(token, out var fields, out var error))
                    return StatusCode(403, error);

                if (!fields.TryGetValue("type", out var type) || type != "EMP")
                    return BadRequest("Invalid token type");

                if (!fields.TryGetValue("empId", out var empIdStr) || !int.TryParse(empIdStr, out empId))
                    return BadRequest("Invalid employee id");
            }
            else
            {
                if (id == null) return NotFound();
                empId = id.Value;
            }

            var emp = await _context.Employees.FirstOrDefaultAsync(x => x.Id == empId);
            if (emp == null) return NotFound();

            ViewBag.PhysicalFolder = Path.Combine(FILE_ROOT, emp.EmployeeCode);

            return View(emp);
        }

        // ================================
        // CREATE (GET)
        // ================================
        // ================================
        public IActionResult Create()
        {
            return View(new Employee { EmployeeCode = GenerateNextEmployeeCode() });
        }

        // ================================
        // CREATE (POST)
        // ================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
     Employee model,
     string? NewDepartment,
     string? NewPosition,
     IFormFile? ProfilePhoto,
     IFormFile? AadhaarFile,
     IFormFile? PanFile,
     IFormFile? PassbookFile,
     List<IFormFile>? ExperienceCertificateFiles,
     IFormFile? MedicalDocumentFile,
     IFormFile? TenthMarksheetFile,
     IFormFile? TwelfthMarksheetFile,
     IFormFile? GraduationMarksheetFile,
     IFormFile? PostGraduationMarksheetFile
 )
        {
            // 🔥 REMOVE FILE VALIDATION AUTO ERRORS
            foreach (var key in ModelState.Keys.ToList())
            {
                if (key.Contains("File") || key.Contains("Marksheet") || key.Contains("Document"))
                    ModelState.Remove(key);
            }

            // APPLY MANUAL FIELDS
            if (!string.IsNullOrWhiteSpace(NewDepartment))
                model.Department = NewDepartment.Trim();

            if (!string.IsNullOrWhiteSpace(NewPosition))
                model.Position = NewPosition.Trim();

            // REQUIRED FIELD HELPER
            void Require(string field, string? value, string message)
            {
                if (string.IsNullOrWhiteSpace(value))
                    ModelState.AddModelError(field, message);
            }

            // REQUIRED VALIDATIONS
            Require("Name", model.Name, "Full Name is required.");
            Require("Email", model.Email, "Email is required.");
            Require("MobileNumber", model.MobileNumber, "Mobile number is required.");
            Require("Gender", model.Gender, "Gender is required.");
            Require("FatherName", model.FatherName, "Father name is required.");
            Require("MotherName", model.MotherName, "Mother name is required.");
            Require("Address", model.Address, "Current address is required.");
            Require("PermanentAddress", model.PermanentAddress, "Permanent address is required.");
            Require("MaritalStatus", model.MaritalStatus, "Marital status is required.");

            Require("Department", model.Department, "Department is required.");
            Require("Position", model.Position, "Position is required.");
            Require("ReportingManager", model.ReportingManager, "Reporting Manager is required.");
            Require("Role", model.Role, "Role is required.");

            Require("AadhaarNumber", model.AadhaarNumber, "Aadhaar Number is required.");
            Require("PanNumber", model.PanNumber, "PAN Number is required.");

            Require("AccountHolderName", model.AccountHolderName, "Account Holder name is required.");
            Require("BankName", model.BankName, "Bank name is required.");
            Require("AccountNumber", model.AccountNumber, "Account number is required.");
            Require("IFSC", model.IFSC, "IFSC code is required.");
            Require("Branch", model.Branch, "Branch is required.");

            Require("EmergencyContactName", model.EmergencyContactName, "Emergency contact name is required.");
            Require("EmergencyContactRelationship", model.EmergencyContactRelationship, "Relationship is required.");
            Require("EmergencyContactMobile", model.EmergencyContactMobile, "Emergency mobile number is required.");
            Require("EmergencyContactAddress", model.EmergencyContactAddress, "Emergency contact address is required.");

            if (model.JoiningDate == null)
                ModelState.AddModelError("JoiningDate", "Joining Date is required.");

            if (model.DOB_Date == null)
                ModelState.AddModelError("DOB_Date", "Date of Birth is required.");

            if (model.Salary == null)
                ModelState.AddModelError("Salary", "Salary is required.");

            if (!ModelState.IsValid)
                return View(model);

            // DEFAULT DATA
            if (string.IsNullOrEmpty(model.EmployeeCode))
                model.EmployeeCode = GenerateNextEmployeeCode();

            model.Status = "Active";
            model.ManagerId = 36;

            // UNIQUE CHECK
            if (await _context.Employees.AnyAsync(x => x.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Email already exists.");
                return View(model);
            }

            if (await _context.Employees.AnyAsync(x => x.MobileNumber == model.MobileNumber))
            {
                ModelState.AddModelError("MobileNumber", "Mobile number already exists.");
                return View(model);
            }

            // 📁 ENSURE EMPLOYEE FOLDER
            string empFolder = Path.Combine(FILE_ROOT, model.EmployeeCode);
            if (!Directory.Exists(empFolder))
                Directory.CreateDirectory(empFolder);

            // SAVE FILES
            model.ProfileImagePath = await SaveEmployeeFile(ProfilePhoto, model.EmployeeCode, "profile");
            model.AadhaarFilePath = await SaveEmployeeFile(AadhaarFile, model.EmployeeCode, "aadhaar");
            model.PanFilePath = await SaveEmployeeFile(PanFile, model.EmployeeCode, "pan");
            model.PassbookFilePath = await SaveEmployeeFile(PassbookFile, model.EmployeeCode, "passbook");

            if (ExperienceCertificateFiles?.Any() == true)
            {
                var saved = new List<string>();
                foreach (var file in ExperienceCertificateFiles)
                    saved.Add(await SaveEmployeeFile(file, model.EmployeeCode, "experience"));

                model.ExperienceCertificateFilePath = string.Join(",", saved);
            }

            model.MedicalDocumentFilePath =
                await SaveEmployeeFile(MedicalDocumentFile, model.EmployeeCode, "medical");

            model.TenthMarksheetFilePath =
                await SaveEmployeeFile(TenthMarksheetFile, model.EmployeeCode, "10th");

            model.TwelfthMarksheetFilePath =
                await SaveEmployeeFile(TwelfthMarksheetFile, model.EmployeeCode, "12th");

            model.GraduationMarksheetFilePath =
                await SaveEmployeeFile(GraduationMarksheetFile, model.EmployeeCode, "grad");

            model.PostGraduationMarksheetFilePath =
                await SaveEmployeeFile(PostGraduationMarksheetFile, model.EmployeeCode, "pg");

            _context.Employees.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id, string token)
        {
            int empId;

            // =====================================================
            // 🔐 TOKEN LOGIC (ADDED – DECRYPT LINK)
            // =====================================================
            if (!string.IsNullOrEmpty(token))
            {
                if (!UrlEncryptionHelper.TryDecryptToken(token, out var fields, out var error))
                    return StatusCode(403, error);

                if (!fields.TryGetValue("type", out var type) || type != "EMP")
                    return BadRequest("Invalid token type");

                if (!fields.TryGetValue("empId", out var empIdStr) ||
                    !int.TryParse(empIdStr, out empId))
                    return BadRequest("Invalid employee token");
            }
            // =====================================================
            // 🔓 ORIGINAL LOGIC (UNCHANGED)
            // =====================================================
            else
            {
                if (id == null)
                    return NotFound();

                empId = id.Value;
            }

            // =====================================================
            // 🔍 LOAD EMPLOYEE (UNCHANGED)
            // =====================================================
            var emp = await _context.Employees.FirstOrDefaultAsync(e => e.Id == empId);
            if (emp == null)
                return NotFound();


            // 🔹 SAFE string cast
            ViewBag.UserRole = emp.Role?.ToString();
            // =====================================================
            // 📋 ORIGINAL VIEWBAG LOAD (UNCHANGED)
            // =====================================================
            ViewBag.Departments = await _context.Employees
                .Where(e => e.Department != null && e.Department.Trim() != "")
                .Select(e => e.Department.Trim())
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();

            ViewBag.Positions = await _context.Employees
                .Where(e => e.Position != null && e.Position.Trim() != "")
                .Select(e => e.Position.Trim())
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();

            // =====================================================
            // 🧾 ORIGINAL VIEWMODEL MAPPING (UNCHANGED)
            // =====================================================
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
                ReportingManager = emp.ReportingManager,
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
                EmergencyContactAddress = emp.EmergencyContactAddress
            };

            return View(vm);
        }


        // ================================
        // EDIT (POST) — TOKEN ENABLED
        // ================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EmployeeEditVm model, string token)
        {
            int empId;

            // =====================================================
            // 🔐 TOKEN LOGIC (ADDED)
            // =====================================================
            if (!string.IsNullOrEmpty(token))
            {
                if (!UrlEncryptionHelper.TryDecryptToken(token, out var fields, out var error))
                    return StatusCode(403, error);

                if (!fields.TryGetValue("type", out var type) || type != "EMP")
                    return BadRequest("Invalid token type");

                if (!fields.TryGetValue("empId", out var empIdStr) ||
                    !int.TryParse(empIdStr, out empId))
                    return BadRequest("Invalid employee token");
            }
            else
            {
                // 🔓 ORIGINAL ID FLOW
                empId = model.Id;
            }

            // =====================================================
            // 🔍 ORIGINAL LOAD EMPLOYEE (UNCHANGED)
            // =====================================================
            var emp = await _context.Employees.FirstOrDefaultAsync(e => e.Id == empId);
            if (emp == null)
                return NotFound();

            // ================================
            // 🔁 ORIGINAL UPDATE LOGIC (UNCHANGED)
            // ================================

            // BASIC
            emp.Name = model.Name;
            emp.Email = model.Email;
            emp.MobileNumber = model.MobileNumber;
            emp.AlternateMobileNumber = model.AlternateMobileNumber;

            // PASSWORD
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
            emp.Gender = model.Gender;
            emp.FatherName = model.FatherName;
            emp.MotherName = model.MotherName;
            emp.DOB_Date = model.DOB_Date;
            emp.MaritalStatus = model.MaritalStatus;
            emp.Address = model.Address;
            emp.PermanentAddress = model.PermanentAddress;

            // EXPERIENCE
            emp.ExperienceType = model.ExperienceType;
            emp.TotalExperienceYears = model.TotalExperienceYears;
            emp.LastCompanyName = model.LastCompanyName;

            // HEALTH
            emp.HasDisease = model.HasDisease;
            emp.DiseaseName = model.DiseaseName;
            emp.DiseaseSince = model.DiseaseSince;
            emp.MedicinesRequired = model.MedicinesRequired;
            emp.DoctorName = model.DoctorName;
            emp.DoctorContact = model.DoctorContact;

            // JOB
            emp.JoiningDate = model.JoiningDate;
            emp.Department = model.Department;
            emp.Position = model.Position;
            emp.Role = model.Role;
            emp.Salary = model.Salary;
            emp.ReportingManager = model.ReportingManager;

            // EDUCATION
            emp.HSCPercent = model.HSCPercent;
            emp.GraduationCourse = model.GraduationCourse;
            emp.GraduationPercent = model.GraduationPercent;
            emp.PostGraduationCourse = model.PostGraduationCourse;
            emp.PostGraduationPercent = model.PostGraduationPercent;

            // ID
            emp.AadhaarNumber = model.AadhaarNumber;
            emp.PanNumber = model.PanNumber;

            // BANK
            emp.AccountHolderName = model.AccountHolderName;
            emp.BankName = model.BankName;
            emp.AccountNumber = model.AccountNumber;
            emp.IFSC = model.IFSC;
            emp.Branch = model.Branch;

            // EMERGENCY
            emp.EmergencyContactName = model.EmergencyContactName;
            emp.EmergencyContactRelationship = model.EmergencyContactRelationship;
            emp.EmergencyContactMobile = model.EmergencyContactMobile;
            emp.EmergencyContactAddress = model.EmergencyContactAddress;

            // FILES
            emp.ProfileImagePath = await SaveEmployeeFile(model.ProfilePhoto, emp.EmployeeCode, "Profile", emp.ProfileImagePath);
            emp.AadhaarFilePath = await SaveEmployeeFile(model.AadhaarFile, emp.EmployeeCode, "Aadhaar", emp.AadhaarFilePath);
            emp.PanFilePath = await SaveEmployeeFile(model.PanFile, emp.EmployeeCode, "Pan", emp.PanFilePath);
            emp.PassbookFilePath = await SaveEmployeeFile(model.PassbookFile, emp.EmployeeCode, "Passbook", emp.PassbookFilePath);
            emp.MedicalDocumentFilePath = await SaveEmployeeFile(model.MedicalDocumentFile, emp.EmployeeCode, "Medical", emp.MedicalDocumentFilePath);

            emp.TwelfthMarksheetFilePath = await SaveEmployeeFile(model.TwelfthMarksheetFile, emp.EmployeeCode, "12th", emp.TwelfthMarksheetFilePath);
            emp.GraduationMarksheetFilePath = await SaveEmployeeFile(model.GraduationMarksheetFile, emp.EmployeeCode, "Graduation", emp.GraduationMarksheetFilePath);
            emp.PostGraduationMarksheetFilePath = await SaveEmployeeFile(model.PostGraduationMarksheetFile, emp.EmployeeCode, "PG", emp.PostGraduationMarksheetFilePath);

            if (model.ExperienceCertificateFiles != null && model.ExperienceCertificateFiles.Any())
            {
                var certFiles = new List<string>();
                foreach (var file in model.ExperienceCertificateFiles)
                {
                    certFiles.Add(await SaveEmployeeFile(file, emp.EmployeeCode, "Experience", null));
                }
                emp.ExperienceCertificateFilePath = string.Join(",", certFiles);
            }

            await _context.SaveChangesAsync();

            TempData["success"] = "Employee updated successfully!";

            // 🔁 Redirect using token if used
            return RedirectToAction("Edit", new
            {
                id = string.IsNullOrEmpty(token) ? (int?)emp.Id : null,
                token = token
            });
        }


        private async Task<string> SaveEmployeeFile(
      IFormFile file, string empCode, string name, string existing = null)
        {
            if (file == null) return existing;

            string uploadDir = Path.Combine(FILE_ROOT, empCode);

            if (!Directory.Exists(uploadDir))
                Directory.CreateDirectory(uploadDir);

            var fileName = $"{name}_{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadDir, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return fileName;
        }


        //public IActionResult ViewDocument(string empCode, string fileName)
        //{
        //    if (string.IsNullOrEmpty(empCode) || string.IsNullOrEmpty(fileName))
        //        return NotFound();

        //    string filePath = Path.Combine(FILE_ROOT, empCode, fileName);
        //    if (!System.IO.File.Exists(filePath))
        //        return NotFound();

        //    return PhysicalFile(filePath, "application/octet-stream");
        //}
        [HttpGet]
        public IActionResult ViewDocument(string empCode, string fileName)
        {
            if (string.IsNullOrEmpty(empCode) || string.IsNullOrEmpty(fileName))
                return NotFound();

            var filePath = Path.Combine(FILE_ROOT, empCode, fileName);

            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var contentType = GetContentType(filePath);

            // 🔥 KEY POINT: INLINE (VIEW IN BROWSER)
            return PhysicalFile(filePath, contentType, enableRangeProcessing: true);
        }

        [HttpGet]
        public IActionResult DownloadDocument(string empCode, string fileName)
        {
            if (string.IsNullOrEmpty(empCode) || string.IsNullOrEmpty(fileName))
                return NotFound();

            var filePath = Path.Combine(FILE_ROOT, empCode, fileName);

            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var contentType = GetContentType(filePath);

            // 🔥 KEY POINT: ATTACHMENT (FORCE DOWNLOAD)
            return File(
                System.IO.File.ReadAllBytes(filePath),
                contentType,
                fileName
            );
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

        [HttpGet]
        public async Task<IActionResult> GetDepartments()
        {
            var departments = await _context.Employees
                .Where(e => e.Department != null && e.Department.Trim() != "")
                .Select(e => e.Department.Trim())
                .Distinct()
                .OrderBy(d => d)
                .ToListAsync();

            return Json(departments);
        }





        public async Task<IActionResult> Dashboard(int? month, int? year)
        {
            var empId = HttpContext.Session.GetInt32("EmployeeId");
            if (!empId.HasValue)
                return RedirectToAction("Login", "Account");

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Id == empId.Value);

            if (employee == null)
                return RedirectToAction("Login", "Account");

            string empCode = employee.EmployeeCode;

            // --------------------------------------------------
            // MONTH SELECTION (default = current month)
            // --------------------------------------------------
            int selectedMonth = month ?? DateTime.Today.Month;
            int selectedYear = year ?? DateTime.Today.Year;

            DateTime monthStart = new DateTime(selectedYear, selectedMonth, 1);
            DateTime monthEnd = monthStart.AddMonths(1).AddDays(-1);

            // --------------------------------------------------
            // WORKING DAYS (exclude Sundays)
            // --------------------------------------------------
            int workingDays = Enumerable.Range(0, (monthEnd - monthStart).Days + 1)
                .Select(d => monthStart.AddDays(d))
                .Count(d => d.DayOfWeek != DayOfWeek.Sunday);

            // --------------------------------------------------
            // PRESENT DAYS
            // --------------------------------------------------
            int presentDays = await _context.Attendances
                .CountAsync(a =>
                    a.Emp_Code == empCode &&
                    a.Date >= monthStart &&
                    a.Date <= monthEnd &&
                    a.InTime.HasValue);

            // --------------------------------------------------
            // LEAVE DAYS (Approved only, overlap-safe)
            // --------------------------------------------------
            int leaveDays = await _context.Leaves
                .Where(l =>
                    l.EmployeeId == empId.Value &&
                    l.OverallStatus == "Approved" &&
                    l.StartDate <= monthEnd &&
                    (l.EndDate ?? l.StartDate) >= monthStart)
                .SumAsync(l => (int)Math.Ceiling(l.TotalDays));

            // --------------------------------------------------
            // HOLIDAY DAYS
            // --------------------------------------------------
            int holidayDays = await _context.Holidays
                .CountAsync(h =>
                    h.HolidayDate >= monthStart &&
                    h.HolidayDate <= monthEnd);

            // --------------------------------------------------
            // ABSENT DAYS
            // --------------------------------------------------
            int absentDays = Math.Max(
                0,
                workingDays - (presentDays + leaveDays + holidayDays)
            );

            // --------------------------------------------------
            // 🎂 BIRTHDAYS (same as HR Home page)
            // --------------------------------------------------
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            var activeEmployees = await _context.Employees
                .Where(e => e.Status == "Active")
                .AsNoTracking()
                .ToListAsync();

            var todaysBirthdays = activeEmployees
                .Where(e =>
                    e.DOB_Date.HasValue &&
                    e.DOB_Date.Value.Month == today.Month &&
                    e.DOB_Date.Value.Day == today.Day)
                .ToList();

            var tomorrowsBirthdays = activeEmployees
                .Where(e =>
                    e.DOB_Date.HasValue &&
                    e.DOB_Date.Value.Month == tomorrow.Month &&
                    e.DOB_Date.Value.Day == tomorrow.Day)
                .ToList();

            // --------------------------------------------------
            // DASHBOARD VIEWMODEL (CREATE ONCE)
            // --------------------------------------------------
            var vm = new EmployeeDashboardViewModel
            {
                Employee = employee,

                // Attendance graph data
                PresentDays = presentDays,
                LeaveDays = leaveDays,
                HolidayDays = holidayDays,
                AbsentDays = absentDays,
                WorkingDays = workingDays,

                // Month selector
                SelectedMonth = selectedMonth,
                SelectedYear = selectedYear,

                // Birthdays
                TodaysBirthdays = todaysBirthdays,
                TomorrowsBirthdays = tomorrowsBirthdays
            };

            // --------------------------------------------------
            // MONTH SELECTOR SUPPORT
            // --------------------------------------------------
            ViewBag.MonthName = monthStart.ToString("MMMM yyyy");
            ViewBag.PrevMonth = monthStart.AddMonths(-1);
            ViewBag.NextMonth = monthStart.AddMonths(1);

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
            ViewBag.UserRole = employee.Role?.ToString();
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

        public IActionResult ExportExcel(string status)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            IQueryable<Employee> query = _context.Employees;

            if (!string.IsNullOrEmpty(status))
            {
                if (string.Equals(status, "Active", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(e => e.Status == "Active");
                }
                else if (string.Equals(status, "Inactive", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(e => e.Status == "Inactive");
                }
            }

            var employees = query.ToList();

            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Employees");

            ws.Cells[1, 1].Value = "Employee Code";
            ws.Cells[1, 2].Value = "Name";
            ws.Cells[1, 3].Value = "Email";
            ws.Cells[1, 4].Value = "Department";
            ws.Cells[1, 5].Value = "Position";
            ws.Cells[1, 6].Value = "Status";

            int row = 2;
            foreach (var emp in employees)
            {
                ws.Cells[row, 1].Value = emp.EmployeeCode;
                ws.Cells[row, 2].Value = emp.Name;
                ws.Cells[row, 3].Value = emp.Email;
                ws.Cells[row, 4].Value = emp.Department;
                ws.Cells[row, 5].Value = emp.Position;
                ws.Cells[row, 6].Value = emp.Status;
                row++;
            }

            ws.Cells[ws.Dimension.Address].AutoFitColumns();

            return File(
                package.GetAsByteArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Employees.xlsx"
            );
        }


        //public IActionResult ExportExcel()
        //    {
        //        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        //        var employees = _context.Employees.ToList();

        //        using (var package = new ExcelPackage())
        //        {
        //            var ws = package.Workbook.Worksheets.Add("Employees");

        //            // HEADER
        //            ws.Cells[1, 1].Value = "Employee Code";
        //            ws.Cells[1, 2].Value = "Name";
        //            ws.Cells[1, 3].Value = "Email";
        //            ws.Cells[1, 4].Value = "Department";
        //            ws.Cells[1, 5].Value = "Position";

        //            int row = 2;

        //            foreach (var emp in employees)
        //            {
        //                ws.Cells[row, 1].Value = emp.EmployeeCode;
        //                ws.Cells[row, 2].Value = emp.Name;
        //                ws.Cells[row, 3].Value = emp.Email;
        //                ws.Cells[row, 4].Value = emp.Department;
        //                ws.Cells[row, 5].Value = emp.Position;

        //                row++;
        //            }

        //            var fileBytes = package.GetAsByteArray();
        //            return File(fileBytes,
        //                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        //                "Employees.xlsx");
        //        }
        //    }

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

        private string GetContentType(string path)
        {
            var ext = Path.GetExtension(path).ToLowerInvariant();

            return ext switch
            {
                ".pdf" => "application/pdf",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                _ => "application/octet-stream"
            };
        }
        // ================================
        // EMPLOYEE REPORT (VIEW ONLY)
        // ================================
        public async Task<IActionResult> EmployeeReport(string search, string status)
        {
            var empId = HttpContext.Session.GetInt32("EmployeeId");
            if (!empId.HasValue)
                return RedirectToAction("Login", "Account");

            var emp = await _context.Employees.FindAsync(empId.Value);
            if (emp == null)
                return RedirectToAction("Login", "Account");

            ViewBag.UserRole = emp.Role;

            // 🔹 Only management roles allowed
            if (emp.Role != "HR" && emp.Role != "GM" && emp.Role != "VP" && emp.Role != "Director")
                return Forbid();

            var query = _context.Employees.AsQueryable();

            // 🔍 Search
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(e =>
                    e.EmployeeCode.Contains(search) ||
                    e.Name.Contains(search) ||
                    e.Email.Contains(search));
            }

            // 🔍 Status filter
            if (!string.IsNullOrWhiteSpace(status) && status != "All")
                query = query.Where(e => e.Status == status);

            var list = await query
                .OrderBy(e => e.EmployeeCode)
                .Select(e => new Employee
                {
                    EmployeeCode = e.EmployeeCode,
                    Name = e.Name,
                    Email = e.Email,
                    Department = e.Department,
                    Position = e.Position,
                    MobileNumber = e.MobileNumber,
                    JoiningDate = e.JoiningDate,
                    Status = e.Status,
                    PanNumber = e.PanNumber,
                    AadhaarNumber= e.AadhaarNumber ,
                    AlternateMobileNumber=  e.AlternateMobileNumber ,
                    Gender=e.Gender ,
                    Address= e.Address ,
                    ReportingManager=e.ReportingManager ,
                    BankName= e.BankName ,
                    AccountNumber= e.AccountNumber,
                    IFSC=e.IFSC ,
                })
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.Status = status;

            return View(list);
        }

    }
}
