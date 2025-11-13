using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using HRMS.Data;
using HRMS.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        // Department → Positions map
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

            var employee = await _context.Employees
                .FirstOrDefaultAsync(m => m.Id == id);

            if (employee == null) return NotFound();

            return View(employee);
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
            // remove ConfirmPassword from model state so Compare works only on client
            ModelState.Remove(nameof(Employee.ConfirmPassword));

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // unique email & mobile checks
            if (await _context.Employees.AnyAsync(e => e.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Email is already registered.");
                return View(model);
            }

            if (await _context.Employees.AnyAsync(e => e.MobileNumber == model.MobileNumber))
            {
                ModelState.AddModelError("MobileNumber", "Mobile number is already registered.");
                return View(model);
            }

            // ensure EmployeeCode
            if (string.IsNullOrWhiteSpace(model.EmployeeCode))
            {
                model.EmployeeCode = GenerateNextEmployeeCode();
            }

            // hash password
            model.Password = HashPassword(model.Password);

            // handle profile image
            if (ProfilePhoto != null && ProfilePhoto.Length > 0)
            {
                model.ProfileImagePath = await SaveProfilePhotoAsync(ProfilePhoto, model.EmployeeCode);
            }

            _context.Add(model);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Employees/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return NotFound();

            // don't send password hash back to UI
            employee.Password = string.Empty;
            employee.ConfirmPassword = string.Empty;

            return View(employee);
        }

        // POST: Employees/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Employee model, IFormFile ProfilePhoto)
        {
            if (id != model.Id) return NotFound();

            // ConfirmPassword not stored
            ModelState.Remove(nameof(Employee.ConfirmPassword));

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return NotFound();

            // unique email/mobile (excluding current)
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

            // update basic fields
            employee.Name = model.Name;
            employee.Email = model.Email;
            employee.MobileNumber = model.MobileNumber;
            employee.Gender = model.Gender;
            employee.FatherName = model.FatherName;
            employee.MotherName = model.MotherName;
            employee.DOB_Date = model.DOB_Date;
            employee.MaritalStatus = model.MaritalStatus;
            employee.ExperienceType = model.ExperienceType;
            employee.TotalExperienceYears = model.TotalExperienceYears;
            employee.LastCompanyName = model.LastCompanyName;
            employee.JoiningDate = model.JoiningDate;
            employee.Department = model.Department;
            employee.Position = model.Position;
            employee.Salary = model.Salary;
            employee.ReportingManager = model.ReportingManager;
            employee.Address = model.Address;
            employee.HSCPercent = model.HSCPercent;
            employee.GraduationCourse = model.GraduationCourse;
            employee.GraduationPercent = model.GraduationPercent;
            employee.PostGraduationCourse = model.PostGraduationCourse;
            employee.PostGraduationPercent = model.PostGraduationPercent;
            employee.AadhaarNumber = model.AadhaarNumber;
            employee.PanNumber = model.PanNumber;

            // if password entered, update hash
            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                employee.Password = HashPassword(model.Password);
            }

            // new image?
            if (ProfilePhoto != null && ProfilePhoto.Length > 0)
            {
                employee.ProfileImagePath = await SaveProfilePhotoAsync(ProfilePhoto, employee.EmployeeCode);
            }

            _context.Update(employee);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Employees/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var employee = await _context.Employees
                .FirstOrDefaultAsync(m => m.Id == id);

            if (employee == null) return NotFound();

            return View(employee);
        }

        // POST: Employees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee != null)
            {
                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // ========= Helpers =========

        private string GenerateNextEmployeeCode()
        {
            // default if no employees: IA0001
            var lastCode = _context.Employees
                .OrderByDescending(e => e.EmployeeCode)
                .Select(e => e.EmployeeCode)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(lastCode))
                return "IA0001";

            // assume format "IA0001"
            var prefix = lastCode.Substring(0, 2);
            var numericPart = lastCode.Substring(2);

            if (!int.TryParse(numericPart, out int number))
            {
                // fallback
                return "IA0001";
            }

            number++;
            return $"{prefix}{number:0000}";
        }

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

            // relative path to serve in <img src="">
            return $"/uploads/profiles/{fileName}";
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

    }
}
