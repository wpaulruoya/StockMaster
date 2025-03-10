﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.ResponseCaching;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Primitives;

namespace StockMaster.Controllers
{
    public class UserController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
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
        public async Task<IActionResult> Register(string fullName, string email, string password, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                ViewBag.ErrorMessage = "Password cannot be empty.";
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
                await _userManager.AddToRoleAsync(user, "User"); // Default role assigned
                TempData["SuccessMessage"] = "Registration successful! You can now log in.";
                return RedirectToAction("Login", "User");
            }

            ViewBag.ErrorMessage = "Registration failed. Please try again.";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Authenticate(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                ViewBag.Error = "No account found with this email.";
                return View("Login");
            }

            var result = await _signInManager.PasswordSignInAsync(user, password, false, false);
            if (!result.Succeeded)
            {
                ViewBag.Error = "Incorrect password. Please try again.";
                return View("Login");
            }

            HttpContext.Session.SetString("UserEmail", email);

            if (await _userManager.IsInRoleAsync(user, "SuperAdmin"))
            {
                return RedirectToAction("Dashboard", "SuperAdmin");
            }
            else if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                return RedirectToAction("Dashboard", "Admin");
            }
            else
            {
                return RedirectToAction("Home", "Home");
            }
        }

        [Authorize(Roles = "Admin, SuperAdmin")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Prevent admins from deleting other admins or super admins
            var roles = await _userManager.GetRolesAsync(user);

            if (User.IsInRole("Admin") && (roles.Contains("Admin") || roles.Contains("SuperAdmin")))
            {
                TempData["ErrorMessage"] = "Admins can only delete normal users.";
                return RedirectToAction("ManageUsers"); // Ensure this action exists
            }

            if (User.IsInRole("SuperAdmin") && roles.Contains("SuperAdmin"))
            {
                TempData["ErrorMessage"] = "SuperAdmins cannot delete other SuperAdmins.";
                return RedirectToAction("ManageUsers");
            }

            // Prevent self-deletion
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser.Id == user.Id)
            {
                TempData["ErrorMessage"] = "You cannot delete your own account.";
                return RedirectToAction("ManageUsers");
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "User deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete user.";
            }

            return RedirectToAction("ManageUsers");
        }



        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            HttpContext.Session.Clear();
            Response.Cookies.Delete(".AspNetCore.Identity.Application");

            // Prevent back navigation after logout
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "-1";

            return RedirectToAction("Index", "Home");
        }

        // Apply cache prevention globally for authenticated pages
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult PreventBackNavigation()
        {
            return View();
        }
    }
}
