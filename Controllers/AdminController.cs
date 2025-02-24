using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using StockMaster.Models;
using static StockMaster.Controllers.SuperAdminController;
using Microsoft.AspNetCore.Authentication;

namespace StockMaster.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SmartStockDbContext _context; // ✅ Inject SmartStockDbContext

        public AdminController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, SmartStockDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context; // ✅ Assign injected context
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

            // Count Total Users (Excluding Super Admin)
            var allUsers = await _userManager.Users.ToListAsync();
            var totalUsers = allUsers.Count(u => !_userManager.GetRolesAsync(u).Result.Contains("SuperAdmin"));

            // Count Total Admins
            var totalAdmins = allUsers.Count(u => _userManager.GetRolesAsync(u).Result.Contains("Admin"));

            // Count Total Inventory Items
            var totalInventory = await _context.Inventories.CountAsync();

            // Fetch Recent Users (excluding SuperAdmin)
            var recentUsers = allUsers
                .Where(u => !_userManager.GetRolesAsync(u).Result.Contains("SuperAdmin"))
                .OrderByDescending(u => u.Id)
                .Take(5)
                .ToList();

            // Pass data to ViewBag
            ViewBag.TotalUsers = totalUsers;
            ViewBag.AdminCount = totalAdmins;
            ViewBag.TotalInventory = totalInventory;
            ViewBag.RecentUsers = recentUsers;

            return View();
        }


        public async Task<IActionResult> ManageUsers()
        {
            try
            {
                var users = await _userManager.Users.ToListAsync();
                var userList = new List<UserWithRoleViewModel>();

                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    if (!roles.Contains("SuperAdmin")) // Exclude SuperAdmin users
                    {
                        userList.Add(new UserWithRoleViewModel
                        {
                            Id = user.Id,
                            Email = user.Email,
                            Role = roles.FirstOrDefault() ?? "User"
                        });
                    }
                }

                // ✅ Sort: Admins first, then Normal Users (based on role)
                userList = userList
                    .OrderByDescending(u => u.Role == "Admin") // Admins appear first
                    .ThenBy(u => u.Email) // Sort alphabetically within role groups
                    .ToList();

                ViewBag.Users = userList;
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
            try
            {
                var inventoryData = (from inv in _context.Inventories
                                     join usr in _context.Users on inv.UserId equals usr.Id into userGroup
                                     from user in userGroup.DefaultIfEmpty()
                                     select new
                                     {
                                         inv.Id,
                                         inv.Name,
                                         inv.Quantity,
                                         inv.Price,
                                         inv.UserId,
                                         UserEmail = user != null ? user.Email : "Unknown" // ✅ Get User Email
                                     }).ToList();

                ViewBag.Users = _context.Users.Select(u => new { u.Id, u.Email }).ToList();
                ViewBag.Inventory = inventoryData;

                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching inventory: {ex.Message}");
                ViewBag.ErrorMessage = "An error occurred while fetching inventory.";
                return View("Error");
            }
        }

        [HttpPost]
        public async Task<JsonResult> EditInventory([FromBody] Inventory model)
        {
            if (model == null || model.Id == 0)
            {
                return Json(new { success = false, message = "Invalid request." });
            }

            var inventoryItem = await _context.Inventories.FindAsync(model.Id);
            if (inventoryItem == null)
            {
                return Json(new { success = false, message = "Inventory item not found." });
            }

            inventoryItem.Name = model.Name;
            inventoryItem.Quantity = model.Quantity;
            inventoryItem.Price = model.Price;

            _context.Inventories.Update(inventoryItem);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Inventory updated successfully." });
        }
        [HttpPost]
        [ValidateAntiForgeryToken] // ✅ Ensures request security
        public async Task<JsonResult> DeleteInventory([FromBody] InventoryDeleteRequest request)
        {
            if (request == null || request.Id == 0)
            {
                return Json(new { success = false, message = "Invalid request." });
            }

            var inventoryItem = await _context.Inventories.FindAsync(request.Id);
            if (inventoryItem == null)
            {
                return Json(new { success = false, message = "Inventory item not found." });
            }

            _context.Inventories.Remove(inventoryItem);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Inventory deleted successfully." });
        }

        // Define a request model for deletion
        public class InventoryDeleteRequest
        {
            public int Id { get; set; }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> DeleteUser([FromBody] UserActionRequest request)
        {
            Console.WriteLine($"Delete request received for User ID: {request?.UserId}"); // ✅ Debugging

            if (request == null || string.IsNullOrEmpty(request.UserId))
            {
                return Json(new { success = false, message = "Invalid request." });
            }

            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            var roles = await _userManager.GetRolesAsync(user);
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser.Id == user.Id)
            {
                return Json(new { success = false, message = "You cannot delete your own account." });
            }

            if (roles.Contains("Admin"))
            {
                return Json(new { success = false, message = "Admins cannot delete other Admins." });
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return Json(new { success = true, message = "User deleted successfully." });
            }
            else
            {
                return Json(new { success = false, message = "Error deleting user. Please try again." });
            }
        }







        public class UserActionRequest
        {
            public string UserId { get; set; }
        }


        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            return RedirectToAction("Index", "Home");
        }

    }

    public class UserWithRoleViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
    }
}
