using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StockMaster.Models;
using System.Linq;
using System.Threading.Tasks;

namespace StockMaster.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SmartStockDbContext _dbContext;

        public AdminController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, SmartStockDbContext dbContext)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _dbContext = dbContext;
        }

        // ✅ Admin Dashboard
        public IActionResult Dashboard()
        {
            var model = new AdminViewModel
            {
                Users = _userManager.Users.ToList(),
                TotalUsers = _userManager.Users.Count(),
                TotalInventoryItems = _dbContext.Inventories.Count(),
                PendingOrders = 45 // Placeholder: Replace with actual logic
            };

            return View(model);
        }


        // ✅ Manage Users Page
        public IActionResult ManageUsers()
        {
            var model = new AdminViewModel
            {
                Users = _userManager.Users.ToList(),
                TotalUsers = _userManager.Users.Count()
            };

            return View(model);
        }


        // ✅ Promote User to Admin
        [HttpPost]
        public async Task<IActionResult> PromoteToAdmin(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            if (!await _userManager.IsInRoleAsync(user, "Admin"))
            {
                await _userManager.AddToRoleAsync(user, "Admin");
            }

            return RedirectToAction("ManageUsers");
        }

        // ✅ Demote Admin to User
        [HttpPost]
        public async Task<IActionResult> DemoteToUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                await _userManager.RemoveFromRoleAsync(user, "Admin");
            }

            return RedirectToAction("ManageUsers");
        }

        // ✅ Delete User
        [HttpPost]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            await _userManager.DeleteAsync(user);
            return RedirectToAction("ManageUsers");
        }

        // ✅ Placeholder for Inventory Management (Extendable)
        public IActionResult ManageInventory()
        {
            var inventoryItems = _dbContext.Inventories.ToList();
            return View(inventoryItems);
        }
    }
}
