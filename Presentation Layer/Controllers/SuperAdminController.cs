using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockMaster.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StockMaster.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class SuperAdminController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SmartStockDbContext _context;

        public SuperAdminController(UserManager<IdentityUser> userManager, SmartStockDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }


        public async Task<IActionResult> Dashboard()
        {
            var totalUsers = _userManager.Users.Count();
            var totalInventory = _context.Inventories.Count();

            // Get all users and check their roles in memory
            var users = _userManager.Users.ToList();
            int adminCount = 0;

            foreach (var user in users)
            {
                if (await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    adminCount++;
                }
            }

            var userCount = totalUsers - adminCount;

            // ✅ Fetch the last 5 registered users (assuming IdentityUser has a CreatedAt field)
            var recentUsers = await _userManager.Users
                .OrderByDescending(u => u.Id) // If CreatedAt is not available, use Id as a fallback
                .Take(5)
                .Select(u => new { u.UserName, u.Email })
                .ToListAsync();

            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalInventory = totalInventory;
            ViewBag.AdminCount = adminCount;
            ViewBag.UserCount = userCount;
            ViewBag.RecentUsers = recentUsers;

            return View();
        }



        // ✅ Manage Users
        public async Task<IActionResult> ManageUsers()
        {
            var users = _userManager.Users.ToList();
            var userList = new List<UserRoleViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userList.Add(new UserRoleViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    Role = roles.FirstOrDefault() ?? "User"
                });
            }

            return View(userList);
        }

        // ✅ Promote User to Admin
        [HttpPost]
        public async Task<IActionResult> PromoteUser([FromBody] UserActionRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                return Json(new { success = false, message = "User is already an Admin." });
            }

            await _userManager.AddToRoleAsync(user, "Admin");
            return Json(new { success = true, message = "User has been promoted to Admin successfully." });
        }

        [HttpPost]
        public async Task<IActionResult> DemoteUser([FromBody] UserActionRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            if (!await _userManager.IsInRoleAsync(user, "Admin"))
            {
                return Json(new { success = false, message = "User is not an Admin." });
            }

            await _userManager.RemoveFromRoleAsync(user, "Admin");
            return Json(new { success = true, message = "User has been demoted to a regular User." });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser([FromBody] UserActionRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            await _userManager.DeleteAsync(user);
            return Json(new { success = true, message = "User has been deleted successfully." });
        }

        // Create a model for handling user actions
        public class UserActionRequest
        {
            public string UserId { get; set; }
        }


        // ✅ Manage Inventory (Fixed)
        public IActionResult ManageInventory()
        {
            var inventories = _context.Inventories.ToList();
            var users = _context.Users.ToList(); // Get users separately

            var groupedInventories = inventories
                .GroupBy(i => i.UserId)
                .Select(g => new
                {
                    UserEmail = users.FirstOrDefault(u => u.Id == g.Key)?.Email ?? "Unknown User",
                    Items = g.ToList()
                })
                .ToList();

            ViewBag.GroupedInventoryItems = groupedInventories;

            return View();
        }






        public async Task<IActionResult> ViewUserInventory(string email)
        {
            // Find the user by email
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("ManageInventory");
            }

            // Get inventory items for this user
            var userInventory = await _context.Inventories
                                              .Where(i => i.UserId == user.Id)
                                              .ToListAsync();

            var inventoryViewModel = userInventory.Select(i => new InventoryViewModel
            {
                Id = i.Id,
                Name = i.Name,
                Description = i.Description,
                Quantity = i.Quantity,
                Price = i.Price,
                UserId = i.UserId,
                UserEmail = email
            }).ToList();

            return View("UserInventory", inventoryViewModel); // Redirects to a new view
        }


        // ✅ Delete Inventory Item
        public async Task<IActionResult> DeleteInventory(int id)
        {
            var item = await _context.Inventories.FindAsync(id);
            if (item == null)
            {
                TempData["Error"] = "Inventory item not found.";
                return RedirectToAction("ManageInventory");
            }

            _context.Inventories.Remove(item);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Inventory item deleted successfully.";
            return RedirectToAction("ManageInventory");
        }
        // In SuperAdminController.cs

        // Change User Password
        [HttpPost]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            if (request.NewPassword != request.NewPassword)
            {
                return Json(new { success = false, message = "Passwords do not match." });
            }

            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, resetToken, request.NewPassword);

            if (result.Succeeded)
            {
                return Json(new { success = true, message = "Password changed successfully." });
            }
            else
            {
                return Json(new { success = false, message = "Error changing password." });
            }
        }


        public class ChangePasswordRequest
        {
            public string UserId { get; set; }
            public string NewPassword { get; set; }
        }
    }

}
