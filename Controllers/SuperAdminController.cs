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

            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalInventory = totalInventory;
            ViewBag.AdminCount = adminCount;
            ViewBag.UserCount = userCount;

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
        public async Task<IActionResult> ManageInventory()
        {
            var inventoryList = await _context.Inventories.ToListAsync();
            var userIds = inventoryList.Select(i => i.UserId).Distinct().ToList();
            var users = await _userManager.Users
                                          .Where(u => userIds.Contains(u.Id))
                                          .ToDictionaryAsync(u => u.Id, u => u.Email);

            var inventoryViewModel = inventoryList.Select(i => new InventoryViewModel
            {
                Id = i.Id,
                Name = i.Name,
                Description = i.Description,
                Quantity = i.Quantity,
                Price = i.Price,
                UserId = i.UserId,
                UserEmail = users.ContainsKey(i.UserId) ? users[i.UserId] : "Unknown"
            }).ToList();

            return View(inventoryViewModel);
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

        // ✅ Change User Password
        public async Task<IActionResult> ChangeUserPassword(string id, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

            if (result.Succeeded)
            {
                return Json(new { success = true, message = "Password updated successfully." });
            }
            else
            {
                return Json(new { success = false, message = "Error updating password." });
            }
        }

    }

}
