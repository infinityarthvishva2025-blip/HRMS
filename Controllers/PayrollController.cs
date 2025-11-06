using Microsoft.AspNetCore.Mvc;
using HRMS.Models;
using System.Collections.Generic;

namespace HRMS.Controllers
{
    public class PayrollController : Controller
    {
        // In-memory data (replace with DB later)
        private static List<Payroll> _payrolls = new List<Payroll>
        {
            new Payroll { Id=1, EmployeeName="John Smith", BasicSalary=37500, HRA=15000, TA=7500, Bonus=3750, Deductions=11250 },
            new Payroll { Id=2, EmployeeName="Sarah Johnson", BasicSalary=32500, HRA=13000, TA=6500, Bonus=3250, Deductions=9750 },
            new Payroll { Id=3, EmployeeName="Mike Wilson", BasicSalary=27500, HRA=11000, TA=5500, Bonus=2750, Deductions=8250 },
            new Payroll { Id=4, EmployeeName="Emily Brown", BasicSalary=30000, HRA=12000, TA=6000, Bonus=3000, Deductions=9000 },
            new Payroll { Id=5, EmployeeName="David Lee", BasicSalary=40000, HRA=16000, TA=8000, Bonus=4000, Deductions=12000 }
        };

        public IActionResult Index()
        {
            var investments = new InvestmentDeclaration();
            ViewBag.Investments = investments;
            return View(_payrolls);
        }

        [HttpPost]
        public IActionResult SubmitDeclaration(InvestmentDeclaration declaration)
        {
            // For now, just display a success message (you can store later)
            ViewBag.Message = "Investment Declaration Submitted Successfully!";
            ViewBag.Investments = declaration;
            return View("Index", _payrolls);
        }
    }
}
