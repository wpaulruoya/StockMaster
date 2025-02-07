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

        [HttpPost]
        public async Task<IActionResult> Add(Inventory model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var userId = _userManager.GetUserId(User); // Get the logged-in user ID
                    if (userId == null)
                    {
                        Console.WriteLine("❌ User ID is null. User is not logged in.");
                        TempData["ErrorMessage"] = "User not found. Please log in again.";
                        return RedirectToAction("Login", "User");
                    }

                    model.UserId = userId; // Ensure the item is linked to the user

                    // 🔴 Debugging: Log the item before saving
                    Console.WriteLine($"✅ Saving Item: Name={model.Name}, Quantity={model.Quantity}, Price={model.Price}, UserId={model.UserId}");

                    _context.Inventories.Add(model);
                    int result = await _context.SaveChangesAsync();

                    if (result > 0)
                    {
                        Console.WriteLine("✅ Item saved successfully!");
                    }
                    else
                    {
                        Console.WriteLine("❌ Item was NOT saved!");
                    }

                    TempData["SuccessMessage"] = "Item added successfully!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error: {ex.Message}");
                    TempData["ErrorMessage"] = "An error occurred while adding the item.";
                }
            }
            else
            {
                Console.WriteLine("❌ Model state is invalid.");
            }
            return View(model);
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
