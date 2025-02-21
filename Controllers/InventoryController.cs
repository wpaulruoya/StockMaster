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
            // Prevent browser from caching the page
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";

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
            // Prevent browser from caching the page
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "User");

            var model = new Inventory { UserId = user.Id };
            return View(model);
        }

        // ✅ Handle adding a new inventory item
        [HttpPost]
        public async Task<IActionResult> Add(Inventory inventory)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "User");

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

        // ✅ Show Edit Inventory Form
        public async Task<IActionResult> Edit(int id)
        {

            // Prevent browser from caching the page
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "User");

            var inventory = await _context.Inventories.FirstOrDefaultAsync(i => i.Id == id && i.UserId == user.Id);
            if (inventory == null)
            {
                TempData["ErrorMessage"] = "Inventory item not found.";
                return RedirectToAction("Index");
            }

            return View(inventory);
        }

        // ✅ Handle editing an inventory item
        [HttpPost]
        public async Task<IActionResult> Edit(Inventory inventory)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "User");

            if (inventory.UserId != user.Id)
            {
                TempData["ErrorMessage"] = "Unauthorized action.";
                return RedirectToAction("Index");
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return View(inventory);
            }

            try
            {
                _context.Inventories.Update(inventory);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Inventory item updated successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Failed to update item: {ex.Message}";
                return View(inventory);
            }
        }

        // ✅ Show Delete Confirmation Page
        public async Task<IActionResult> Delete(int id)
        {

            // Prevent browser from caching the page
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "User");

            var inventory = await _context.Inventories.FirstOrDefaultAsync(i => i.Id == id && i.UserId == user.Id);
            if (inventory == null)
            {
                TempData["ErrorMessage"] = "Inventory item not found.";
                return RedirectToAction("Index");
            }

            return View(inventory);
        }

        // ✅ Handle deletion of an inventory item
        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "User");

            var inventory = await _context.Inventories.FirstOrDefaultAsync(i => i.Id == id && i.UserId == user.Id);
            if (inventory == null)
            {
                TempData["ErrorMessage"] = "Inventory item not found.";
                return RedirectToAction("Index");
            }

            try
            {
                _context.Inventories.Remove(inventory);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Inventory item deleted successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Failed to delete item: {ex.Message}";
                return RedirectToAction("Index");
            }
        }
    }
}
