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
    [Authorize] // Ensure only logged-in users can access inventory
    public class InventoryController : Controller
    {
        private readonly SmartStockDbContext _context;

        public InventoryController(SmartStockDbContext context)
        {
            _context = context;
        }


        // GET: Inventory
        public IActionResult Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get logged-in user ID
            var userInventory = _context.Inventories.Where(i => i.UserId == userId).ToList(); // Get user's inventory items

            return View(userInventory); // Pass the inventory list to the view
        }


        // GET: Inventory/Add
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
                    model.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Assign user ID
                    _context.Inventories.Add(model);
                    await _context.SaveChangesAsync();

                    // Set success message
                    TempData["SuccessMessage"] = "Item added successfully!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    // Log error and show message
                    Console.WriteLine($"Error: {ex.Message}");
                    TempData["ErrorMessage"] = "An error occurred while adding the item.";
                    return RedirectToAction("Index");
                }
            }
            return View(model);
        }


        // GET: Inventory/Edit/{id}
        public IActionResult Edit(int id)
        {
            var item = _context.Inventories.Find(id);
            if (item == null || item.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                return NotFound();
            }
            return View(item);
        }

        // POST: Inventory/Edit
        [HttpPost]
        public async Task<IActionResult> Edit(Inventory model)
        {
            if (ModelState.IsValid)
            {
                var item = _context.Inventories.Find(model.Id);
                if (item == null || item.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
                {
                    return NotFound();
                }

                item.Name = model.Name;
                item.Quantity = model.Quantity;
                item.Price = model.Price;

                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(model);
        }

        // GET: Inventory/Delete/{id}
        public IActionResult Delete(int id)
        {
            var item = _context.Inventories.Find(id);
            if (item == null || item.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                return NotFound();
            }
            return View(item);
        }

        // POST: Inventory/Delete
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = _context.Inventories.Find(id);
            if (item == null || item.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                return NotFound();
            }

            _context.Inventories.Remove(item);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
