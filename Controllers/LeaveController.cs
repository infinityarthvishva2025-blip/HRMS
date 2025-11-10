using HRMS.Data;
using HRMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Controllers
{
    public class LeaveController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LeaveController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Leave/Index
        public IActionResult Index()
        {
            var leaves = _context.Leaves.AsNoTracking().ToList();
            return View(leaves);
        }

        // GET: Leave/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Leave/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Leave leave)
        {
            if (ModelState.IsValid)
            {
                // Assign default status and employee code if not provided
                leave.Status = "Pending";
                if (string.IsNullOrEmpty(leave.EmployeeCode))
                    leave.EmployeeCode = "EMP001"; // Placeholder if user not logged in

                _context.Leaves.Add(leave);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Leave request submitted successfully!";
                return RedirectToAction(nameof(Index));
            }

            // If validation fails, redisplay the form
            return View(leave);
        }

        // GET: Leave/Edit/{id}
        public IActionResult Edit(int id)
        {
            var leave = _context.Leaves.Find(id);
            if (leave == null)
                return NotFound();

            return View(leave);
        }

        // POST: Leave/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Leave leave)
        {
            if (id != leave.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(leave);
                    _context.SaveChanges();
                    TempData["SuccessMessage"] = "Leave updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Leaves.Any(e => e.Id == leave.Id))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(leave);
        }

        // POST: Leave/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var leave = _context.Leaves.Find(id);
            if (leave == null)
                return NotFound();

            _context.Leaves.Remove(leave);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Leave deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
