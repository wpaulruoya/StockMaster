using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockMaster.Models;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

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

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return RedirectToAction("Login", "User");
            }

            var inventoryItems = await _context.Inventories
                .AsNoTracking() // Ensure EF doesn't cache stale data
                .Where(i => i.UserId == userId)
                .ToListAsync();

            Console.WriteLine($"Found {inventoryItems.Count} items for user {userId}");

            return View(inventoryItems);
        }


        public IActionResult Add()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Add(Inventory inventory)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get logged-in user's ID
            Console.WriteLine($"Logged-in User ID: {userId}");

            if (string.IsNullOrEmpty(userId))
            {
                ModelState.AddModelError("", "User not found. Please log in.");
                return View(inventory);
            }

            if (!ModelState.IsValid)
            {
                Console.WriteLine("Model state is invalid.");
                return View(inventory);
            }

            inventory.UserId = userId;
            _context.Inventories.Add(inventory);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }





        public IActionResult Edit(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var item = _context.Inventories.Find(id);

            if (item == null || item.UserId != userId)
            {
                return NotFound();
            }

            return View(item);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Inventory model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var item = _context.Inventories.Find(model.Id);

            if (item == null || item.UserId != userId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                item.Name = model.Name;
                item.Quantity = model.Quantity;
                item.Price = model.Price;

                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(model);
        }

        public IActionResult Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var item = _context.Inventories.Find(id);

            if (item == null || item.UserId != userId)
            {
                return NotFound();
            }

            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var item = _context.Inventories.Find(id);

            if (item == null || item.UserId != userId)
            {
                return NotFound();
            }

            _context.Inventories.Remove(item);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
