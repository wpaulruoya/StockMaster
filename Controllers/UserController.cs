using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace StockMaster.Controllers
{
    public class UserController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        public UserController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> Register(string fullName, string email, string password, string confirmPassword)
        {
            if (password != confirmPassword)
            {
                ViewBag.ErrorMessage = "Passwords do not match.";
                return View();
            }

            if (password != confirmPassword)
            {
                ViewBag.ErrorMessage = "Passwords do not match.";
                return View();
            }


            var user = new IdentityUser { UserName = email, Email = email };
            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Inventory");
            }

            else
            {
                ViewBag.ErrorMessage = "Registration failed. Please try again.";
                return View();
            }
        }


        [HttpPost]
        public async Task<IActionResult> Authenticate(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                var result = await _signInManager.PasswordSignInAsync(user, password, false, false);
                if (result.Succeeded)
                {
                    HttpContext.Session.SetString("UserEmail", email);
                    return RedirectToAction("Home", "Home");
                }
            }

            ViewBag.Error = "Invalid email or password.";
            return View("Login");
        }
        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Clear session data
            return RedirectToAction("Index", "Home"); // Redirect to landing page
        }

    }
}
