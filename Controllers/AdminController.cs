using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace StockMaster.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        private void PreventPageCaching()
        {
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";
        }

        public async Task<IActionResult> Dashboard()
        {
            PreventPageCaching();

            var recentUsers = await _userManager.Users.OrderByDescending(u => u.Id).Take(5).ToListAsync();
            ViewBag.RecentUsers = recentUsers;

            return View();
        }

        public async Task<IActionResult> ManageUsers()
        {
            try
            {
                var users = await _userManager.Users.ToListAsync();
                var filteredUsers = new List<IdentityUser>();

                foreach (var user in users)
                {
                    if (!await _userManager.IsInRoleAsync(user, "SuperAdmin"))
                    {
                        filteredUsers.Add(user);
                    }
                }

                ViewBag.Users = filteredUsers; // ✅ Make sure ViewBag.Users is always set

                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching users: {ex.Message}");
                ViewBag.ErrorMessage = "An error occurred while fetching users.";
                return View("Error");
            }
        }


        public IActionResult ManageInventory()
        {
            PreventPageCaching();
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Clears session data
            return RedirectToAction("Index", "Home"); // Redirects to the login page
        }
    }
}
