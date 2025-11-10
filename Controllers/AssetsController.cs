using HRMS.Data;
using HRMS.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace HRMS.Controllers
{
    [Route("Assets")]
    [Route("[controller]/[action]")]
    public class AssetsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AssetsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var assets = _context.Assets.ToList();
            return View(assets);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Assets asset)
        {
            if (ModelState.IsValid)
            {
                _context.Assets.Add(asset);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(asset);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var asset = _context.Assets.Find(id);
            return View(asset);
        }

        [HttpPost]
        public IActionResult Edit(Assets asset)
        {
            if (ModelState.IsValid)
            {
                _context.Assets.Update(asset);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(asset);
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            var asset = _context.Assets.Find(id);
            return View(asset);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var asset = _context.Assets.Find(id);
            if (asset != null)
            {
                _context.Assets.Remove(asset);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}
