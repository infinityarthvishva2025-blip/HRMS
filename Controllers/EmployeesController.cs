using HRMS.Models;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.Controllers
{
   



    public class EmployeesController : Controller
    {
        // Temporary static list for demo
        private static List<Employee> employees = new List<Employee>
        {
            new Employee{ Id=1, EmployeeCode="EMP0047", Name="John Smith", Email="john.smith@company.com", Department="IT", Position="Software Developer", Salary=75000, JioTag="JIO0047"},
            new Employee{ Id=2, EmployeeCode="EMP0046", Name="Sarah Johnson", Email="sarah.j@company.com", Department="HR", Position="HR Manager", Salary=65000, JioTag="JIO0046"},
            new Employee{ Id=3, EmployeeCode="EMP0045", Name="Mike Wilson", Email="mike.w@company.com", Department="Finance", Position="Accountant", Salary=55000, JioTag="JIO0045"},
            new Employee{ Id=4, EmployeeCode="EMP0044", Name="Emily Brown", Email="emily.b@company.com", Department="Marketing", Position="Marketing Specialist", Salary=60000, JioTag="JIO0044"},
            new Employee{ Id=5, EmployeeCode="EMP0043", Name="Gaurav Jamdade", Email="david.lee@company.com", Department="Operations", Position="Operations Manager", Salary=80000, JioTag="Not Assigned"}
        };

        public IActionResult Index()
        {
            return View(employees);
        }

        // Add Employee
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Employee model)
        {
            model.Id = employees.Max(e => e.Id) + 1;
            employees.Add(model);
            return RedirectToAction("Index");
        }

        // Edit Employee
        public IActionResult Edit(int id)
        {
            var emp = employees.FirstOrDefault(e => e.Id == id);
            if (emp == null) return NotFound();
            return View(emp);
        }

        [HttpPost]
        public IActionResult Edit(Employee updated)
        {
            var emp = employees.FirstOrDefault(e => e.Id == updated.Id);
            if (emp == null) return NotFound();

            emp.Name = updated.Name;
            emp.Email = updated.Email;
            emp.Department = updated.Department;
            emp.Position = updated.Position;
            emp.Salary = updated.Salary;
            emp.EmployeeCode = updated.EmployeeCode;
            emp.JioTag = updated.JioTag;

            return RedirectToAction("Index");
        }

        // Delete Employee
        public IActionResult Delete(int id)
        {
            var emp = employees.FirstOrDefault(e => e.Id == id);
            if (emp != null) employees.Remove(emp);
            return RedirectToAction("Index");
        }
    }
}
