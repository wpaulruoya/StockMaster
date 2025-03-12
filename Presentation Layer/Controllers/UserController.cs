using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using StockMaster.ApplicationLayer.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace StockMaster.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly UserManager<IdentityUser> _userManager;

        public UserController(IUserService userService, UserManager<IdentityUser> userManager)
        {
            _userService = userService;
            _userManager = userManager;
        }

        public IActionResult Login() => View();
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(string username, string email, string password, string confirmPassword)
        {
            var result = await _userService.RegisterUser(username, email, password, confirmPassword);

            if (result.Succeeded)
            {
                ViewBag.SuccessMessage = "Registration successful! You can now log in.";
                return RedirectToAction("Login");
            }

            ViewBag.ErrorMessage = string.Join(", ", result.Errors.Select(e => e.Description));
            return View();
        }



        [HttpPost]
        public async Task<IActionResult> Authenticate(string email, string password)
        {
            var result = await _userService.AuthenticateUser(email, password);
            if (!result.Succeeded)
            {
                ViewBag.Error = "Invalid login attempt.";
                return View("Login");
            }

            var user = await _userManager.FindByEmailAsync(email);
            HttpContext.Session.SetString("UserEmail", email);

            if (await _userManager.IsInRoleAsync(user, "SuperAdmin"))
                return RedirectToAction("Dashboard", "SuperAdmin");
            if (await _userManager.IsInRoleAsync(user, "Admin"))
                return RedirectToAction("Dashboard", "Admin");

            return RedirectToAction("Home", "Home");
        }

        [Authorize(Roles = "Admin, SuperAdmin")]
        public async Task<IActionResult> Delete(string id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (await _userService.DeleteUser(id, currentUser))
            {
                TempData["SuccessMessage"] = "User deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Deletion failed.";
            }

            return RedirectToAction("ManageUsers");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _userService.LogoutUser();
            HttpContext.Session.Clear();
            Response.Cookies.Delete(".AspNetCore.Identity.Application");

            return RedirectToAction("Index", "Home");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult PreventBackNavigation()
        {
            return View();
        }
    }
}
