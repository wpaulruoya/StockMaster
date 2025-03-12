using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using StockMaster.Models;
using System.Linq;
using System.Threading.Tasks;

namespace StockMaster.Controllers
{
    [Authorize]
    public class InventoryController : Controller
    {
        private readonly IInventoryService _inventoryService;
        private readonly UserManager<IdentityUser> _userManager;

        public InventoryController(IInventoryService inventoryService, UserManager<IdentityUser> userManager)
        {
            _inventoryService = inventoryService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "User");

            var userInventory = await _inventoryService.GetUserInventoryAsync(user.Id);
            return View(userInventory);
        }

        public async Task<IActionResult> Add()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "User");

            var model = new Inventory { UserId = user.Id };
            return View(model);
        }

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

            if (await _inventoryService.AddInventoryAsync(inventory))
            {
                TempData["SuccessMessage"] = "Inventory item added successfully!";
                return RedirectToAction("Index");
            }

            TempData["ErrorMessage"] = "Failed to add inventory item.";
            return View(inventory);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "User");

            var inventory = await _inventoryService.GetInventoryByIdAsync(id, user.Id);
            if (inventory == null)
            {
                TempData["ErrorMessage"] = "Inventory item not found.";
                return RedirectToAction("Index");
            }

            return View(inventory);
        }

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

            if (await _inventoryService.UpdateInventoryAsync(inventory))
            {
                TempData["SuccessMessage"] = "Inventory item updated successfully!";
                return RedirectToAction("Index");
            }

            TempData["ErrorMessage"] = "Failed to update inventory item.";
            return View(inventory);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "User");

            var inventory = await _inventoryService.GetInventoryByIdAsync(id, user.Id);
            if (inventory == null)
            {
                TempData["ErrorMessage"] = "Inventory item not found.";
                return RedirectToAction("Index");
            }

            return View(inventory);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "User");

            if (await _inventoryService.DeleteInventoryAsync(id, user.Id))
            {
                TempData["SuccessMessage"] = "Inventory item deleted successfully!";
                return RedirectToAction("Index");
            }

            TempData["ErrorMessage"] = "Failed to delete inventory item.";
            return RedirectToAction("Index");
        }
    }
}
