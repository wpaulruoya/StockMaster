using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace StockMaster.Controllers
{
    [Authorize(Roles = "Admin")] // Ensures only Admins can access this controller
    public class AdminController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            var recentUsers = await _userManager.Users.OrderByDescending(u => u.Id).Take(5).ToListAsync();
            ViewBag.RecentUsers = recentUsers; // ✅ Ensure ViewBag.RecentUsers is always set

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

                return View(filteredUsers);
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
            return View();
        }
    }
}
