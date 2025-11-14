using HRMS.Data;
using HRMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class LeaveController : Controller
{
    private readonly ApplicationDbContext _context;

    public LeaveController(ApplicationDbContext context)
    {
        _context = context;
    }
    [HttpGet]
    public IActionResult Index()
    {
        var leaves = _context.Leaves
            .Include(l => l.Employee)  // <-- include employee data
            .ToList();

        return View(leaves);
    }

    // SHOW THE FORM
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    // SAVE LOGIC
    [HttpPost]
    public IActionResult Create(Leave model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Auto calculate total leave days
        model.TotalDays = (model.EndDate - model.StartDate).Days + 1;

        // Default status
        model.Status = "Pending";

        // You can set employee ID from login session; for now, static:
        model.EmployeeId = 1;

        _context.Leaves.Add(model);
        _context.SaveChanges();

        TempData["Success"] = "Leave request submitted successfully!";
        return RedirectToAction("Index", "Leave");
    }
}

//using HRMS.Data;
//using HRMS.Models;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;

//namespace HRMS.Controllers
//{
//    public class LeaveController : Controller
//    {
//        private readonly ApplicationDbContext _context;

//        public LeaveController(ApplicationDbContext context)
//        {
//            _context = context;
//        }

//        // ============================================================
//        // INDEX - List all leave applications (HR view)
//        // ============================================================
//        [HttpGet]
//        public IActionResult Index()
//        {
//            // Only HR can see all employee leaves
//            var role = HttpContext.Session.GetString("Role");
//            if (role != "HR")
//                return RedirectToAction("Login", "Account");

//            var leaves = _context.Leaves
//                .Include(l => l.Employee)
//                .OrderByDescending(l => l.StartDate)
//                .ToList();

//            return View(leaves);
//        }

//        // ============================================================
//        // CREATE - Employee creates leave request
//        // ============================================================
//        [HttpGet]
//        public IActionResult Create() => View();

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public IActionResult Create(Leave leave)
//        {
//            if (!ModelState.IsValid) return View(leave);

//            var empId = HttpContext.Session.GetInt32("EmployeeId");
//            if (empId == null) return RedirectToAction("Login", "Account");

//            leave.EmployeeId = empId.Value;
//            leave.Status = "Pending";

//            _context.Leaves.Add(leave);
//            _context.SaveChanges();

//            // Redirect to employee dashboard after submission
//            return RedirectToAction("Dashboard", "Employees");
//        }

//        // ============================================================
//        // MY LEAVES - Employee view of own leave requests
//        // ============================================================
//        [HttpGet]
//        public IActionResult MyLeaves()
//        {
//            var empId = HttpContext.Session.GetInt32("EmployeeId");
//            if (empId == null) return RedirectToAction("Login", "Account");

//            var leaves = _context.Leaves
//                .Include(l => l.Employee)
//                .Where(l => l.EmployeeId == empId.Value)
//                .OrderByDescending(l => l.StartDate)
//                .ToList();

//            return View(leaves);
//        }

//        // ============================================================
//        // UPDATE STATUS - HR approves/rejects leave
//        // ============================================================
//        [HttpPost]
//        public IActionResult UpdateStatus(int id, string status)
//        {
//            var role = HttpContext.Session.GetString("Role");
//            if (role != "HR")
//                return RedirectToAction("Login", "Account");

//            var leave = _context.Leaves.FirstOrDefault(l => l.Id == id);
//            if (leave != null)
//            {
//                leave.Status = status;
//                _context.SaveChanges();
//            }

//            return RedirectToAction(nameof(Index));
//        }
//    }
//}
