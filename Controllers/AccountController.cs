using Microsoft.AspNetCore.Mvc;
using HRMS.Models;

namespace HRMS.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Simple demo authentication (replace with DB check)
                if (model.Username == "admin" && model.Password == "123")
                {
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError("", "Invalid username or password");
            }

            return View(model);
        }
    }
}
