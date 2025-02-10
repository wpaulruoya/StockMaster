using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StockMaster.Models;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace StockMaster.Controllers
{
    [Authorize]
    public class InventoryController : Controller
    {
        private readonly SmartStockDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public InventoryController(SmartStockDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ✅ View all inventory items for the logged-in user
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "User");

            var userInventory = await _context.Inventories
                .Where(i => i.UserId == user.Id)
                .ToListAsync();

            return View(userInventory);
        }

        // ✅ Show Add Inventory Form with pre-filled UserId
        public async Task<IActionResult> Add()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "User");

            var model = new Inventory { UserId = user.Id }; // Ensure UserId is pre-filled
            return View(model);
        }

        // ✅ Handle adding a new inventory item
        [HttpPost]
        public async Task<IActionResult> Add(Inventory inventory)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "User");

            // Assign UserId before checking ModelState
            inventory.UserId = user.Id;

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return View(inventory);
            }

            try
            {
                _context.Inventories.Add(inventory);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Inventory item added successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Failed to add item: {ex.Message}";
                return View(inventory);
            }
        }
    }
}
