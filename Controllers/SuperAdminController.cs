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

        public IActionResult Dashboard()
        {
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
        public async Task<IActionResult> PromoteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("ManageUsers");
            }

            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                TempData["Warning"] = "User is already an Admin.";
                return RedirectToAction("ManageUsers");
            }

            await _userManager.AddToRoleAsync(user, "Admin");
            TempData["Success"] = "User has been promoted to Admin successfully.";
            return RedirectToAction("ManageUsers");
        }

        // ✅ Demote Admin to User
        public async Task<IActionResult> DemoteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("ManageUsers");
            }

            if (!await _userManager.IsInRoleAsync(user, "Admin"))
            {
                TempData["Warning"] = "User is not an Admin.";
                return RedirectToAction("ManageUsers");
            }

            await _userManager.RemoveFromRoleAsync(user, "Admin");
            TempData["Success"] = "User has been demoted to a regular User.";
            return RedirectToAction("ManageUsers");
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
    }
}
