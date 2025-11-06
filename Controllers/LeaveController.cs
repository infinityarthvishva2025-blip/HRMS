using Microsoft.AspNetCore.Mvc;
using HRMS.Models;
using System.Collections.Generic;
using System.Linq;

namespace HRMS.Controllers
{
    public class LeaveController : Controller
    {
        // Temporary static list instead of database
        private static List<Leave> _leaves = new List<Leave>();

        // GET: /Leave/
        public IActionResult Index()
        {
            return View(_leaves);
        }

        // GET: /Leave/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Leave/Create
        [HttpPost]
        public IActionResult Create(Leave leave)
        {
            if (ModelState.IsValid)
            {
                leave.Id = _leaves.Count + 1; // simulate auto ID
                _leaves.Add(leave);
                return RedirectToAction(nameof(Index));
            }
            return View(leave);
        }

        // GET: /Leave/Details/5
        public IActionResult Details(int id)
        {
            var leave = _leaves.FirstOrDefault(x => x.Id == id);
            if (leave == null)
                return NotFound();
            return View(leave);
        }

        // GET: /Leave/Edit/5
        public IActionResult Edit(int id)
        {
            var leave = _leaves.FirstOrDefault(x => x.Id == id);
            if (leave == null)
                return NotFound();
            return View(leave);
        }

        // POST: /Leave/Edit
        [HttpPost]
        public IActionResult Edit(Leave leave)
        {
            var existing = _leaves.FirstOrDefault(x => x.Id == leave.Id);
            if (existing != null)
            {
                existing.LeaveType = leave.LeaveType;
                existing.StartDate = leave.StartDate;
                existing.EndDate = leave.EndDate;
                existing.Reason = leave.Reason;
                existing.Status = leave.Status;
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: /Leave/Delete/5
        public IActionResult Delete(int id)
        {
            var leave = _leaves.FirstOrDefault(x => x.Id == id);
            if (leave != null)
                _leaves.Remove(leave);
            return RedirectToAction(nameof(Index));
        }
    }
}
