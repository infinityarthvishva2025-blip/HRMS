using HRMS.Data;
using HRMS.Models;
using Microsoft.AspNetCore.Mvc;
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

        // GET: /Leave/
        public IActionResult Index()
        {
            var leaves = _context.Leaves.ToList();
            return View(leaves);
        }

        // GET: /Leave/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Leave/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Leave leave)
        {
            if (ModelState.IsValid)
            {
                _context.Leaves.Add(leave);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(leave);
        }

        // GET: /Leave/Edit/5
        public IActionResult Edit(int id)
        {
            var leave = _context.Leaves.Find(id);
            if (leave == null)
                return NotFound();

            return View(leave);
        }

        // POST: /Leave/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Leave leave)
        {
            if (ModelState.IsValid)
            {
                _context.Leaves.Update(leave);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(leave);
        }

        // GET: /Leave/Delete/5
        public IActionResult Delete(int id)
        {
            var leave = _context.Leaves.Find(id);
            if (leave == null)
                return NotFound();

            _context.Leaves.Remove(leave);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        // GET: /Leave/Details/5
        public IActionResult Details(int id)
        {
            var leave = _context.Leaves.Find(id);
            if (leave == null)
                return NotFound();

            return View(leave);
        }
    }
}
