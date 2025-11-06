using HRMS.Data;
using HRMS.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace HRMS.Controllers
{
    public class AssetsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AssetsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var assets = _context.Assets.ToList();

            // For chart summary
            var summary = assets
                .GroupBy(a => a.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToList();

            ViewBag.Summary = summary;
            return View(assets);
        }

        [HttpPost]
        public IActionResult Assign(Asset asset)
        {
            if (ModelState.IsValid)
            {
                _context.Assets.Add(asset);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View("Index");
        }

        public IActionResult Edit(int id)
        {
            var asset = _context.Assets.Find(id);
            if (asset == null)
                return NotFound();
            return View(asset);
        }

        [HttpPost]
        public IActionResult Edit(Asset asset)
        {
            if (ModelState.IsValid)
            {
                _context.Assets.Update(asset);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(asset);
        }

        public IActionResult Delete(int id)
        {
            var asset = _context.Assets.Find(id);
            if (asset != null)
            {
                _context.Assets.Remove(asset);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
