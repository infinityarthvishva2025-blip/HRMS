using HRMS.Data;
using HRMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

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
            var leaves = _context.Leaves.ToList();
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
                _context.Leaves.Add(leave);
                _context.SaveChanges();
                TempData["Success"] = "Leave request submitted successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(leave);
        }

        // GET: Leave/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null) return NotFound();

            var leave = _context.Leaves.Find(id);
            if (leave == null) return NotFound();

            return View(leave);
        }

        // POST: Leave/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Leave leave)
        {
            if (id != leave.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(leave);
                    _context.SaveChanges();
                    TempData["Success"] = "Leave updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Leaves.Any(e => e.Id == leave.Id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(leave);
        }

        // GET: Leave/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null) return NotFound();

            var leave = _context.Leaves.FirstOrDefault(m => m.Id == id);
            if (leave == null) return NotFound();

            return View(leave);
        }

        // POST: Leave/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var leave = _context.Leaves.Find(id);
            _context.Leaves.Remove(leave);
            _context.SaveChanges();
            TempData["Success"] = "Leave deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
