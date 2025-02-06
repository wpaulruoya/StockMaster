using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace StockMaster.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Authenticate(string email, string password)
        {
            // Placeholder authentication logic (Replace with actual database verification)
            if (email == "admin@example.com" && password == "password")
            {
                // Set session or authentication state if needed
                HttpContext.Session.SetString("UserEmail", email);

                // Redirect to Home page after successful login
                return RedirectToAction("Home", "Home");
            }

            // If authentication fails, return to Login page with error message
            ViewBag.Error = "Invalid email or password.";
            return View("Login");
        }
    }
}
