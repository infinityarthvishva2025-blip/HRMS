using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRMS.Data;
using HRMS.Models;

namespace HRMS.Controllers
{
    public class AssetsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AssetsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Assets
        public async Task<IActionResult> Index()
        {
            return View(await _context.Assets.ToListAsync());
        }

        // GET: Assets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var asset = await _context.Assets
                .FirstOrDefaultAsync(m => m.Id == id);

            if (asset == null)
            {
                return NotFound();
            }

            return View(asset);
        }

        // GET: Assets/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Assets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,AssetName,Category,SerialNo,RAM,Storage,PurchaseDate,Cost,Status,AssignedTo,Remarks")] Assets asset)
        {
            if (ModelState.IsValid)
            {
                _context.Add(asset);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(asset);
        }

        // GET: Assets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var asset = await _context.Assets.FindAsync(id);
            if (asset == null)
            {
                return NotFound();
            }
            return View(asset);
        }

        // POST: Assets/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,AssetName,Category,SerialNo,RAM,Storage,PurchaseDate,Cost,Status,AssignedTo,Remarks")] Assets asset)
        {
            if (id != asset.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(asset);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AssetExists(asset.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(asset);
        }

        // GET: Assets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var asset = await _context.Assets
                .FirstOrDefaultAsync(m => m.Id == id);
            if (asset == null)
            {
                return NotFound();
            }

            return View(asset);
        }

        // POST: Assets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var asset = await _context.Assets.FindAsync(id);
            if (asset != null)
            {
                _context.Assets.Remove(asset);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Assets/Request
        public IActionResult Request()
        {
            return View();
        }

        // POST: Assets/Request
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Request(Assets model)
        {
            if (ModelState.IsValid)
            {
                _context.Assets.Add(model);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Your asset request has been submitted successfully!";
                return RedirectToAction("Dashboard", "Employee");
            }
            return View(model);
        }

        // HR view: see all employee asset requests
        public IActionResult RequestsList()
        {
            var requests = _context.Assets.ToList();
            return View(requests);
        }

        // HR update status (approve/reject)
        [HttpPost]
        public IActionResult UpdateStatus(int id, string status)
        {
            var req = _context.Assets.FirstOrDefault(r => r.Id == id);
            if (req != null)
            {
                req.Status = status;
                _context.SaveChanges();
            }
            return RedirectToAction("RequestsList");
        }


        //[HttpPost]
        //public IActionResult UpdateStatus(int id, string status)
        //{
        //    var asset = _context.Assets.FirstOrDefault(a => a.Id == id);
        //    if (asset != null)
        //    {
        //        asset.Status = status;
        //        _context.Update(asset);
        //        _context.SaveChanges();
        //    }
        //    return RedirectToAction("Index");
        //}

        private bool AssetExists(int id)
        {
            return _context.Assets.Any(e => e.Id == id);
        }
    }
}
