using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StockMaster.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StockMaster.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryApiController : ControllerBase
    {
        private readonly SmartStockDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<InventoryApiController> _logger;

        public InventoryApiController(SmartStockDbContext context, UserManager<IdentityUser> userManager, ILogger<InventoryApiController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // ✅ VIEW INVENTORY ITEMS FOR LOGGED-IN USER
        [HttpGet]
        public async Task<ActionResult<object>> GetInventory()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogWarning("Unauthorized access attempt.");
                return Unauthorized(new
                {
                    success = false,
                    message = "User not found or not logged in."
                });
            }

            _logger.LogInformation($"Fetching inventory for user ID: {user.Id}");

            var inventory = await _context.Inventories
                                          .Where(i => i.UserId == user.Id)
                                          .ToListAsync();

            if (!inventory.Any())
            {
                _logger.LogInformation($"No inventory found for user ID: {user.Id}");
                return NotFound(new
                {
                    success = false,
                    message = "No inventory items found.",
                    userId = user.Id
                });
            }

            return Ok(new
            {
                success = true,
                message = "Inventory retrieved successfully.",
                totalItems = inventory.Count,
                inventory
            });
        }

        // ✅ VIEW INVENTORY ITEMS FOR A SPECIFIC USER BY ID
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<object>> GetInventoryByUserId(string userId)
        {
            _logger.LogInformation($"Fetching inventory for user: {userId}");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning($"User {userId} not found.");
                return NotFound(new
                {
                    success = false,
                    message = "User not found.",
                    userId
                });
            }

            var inventory = await _context.Inventories
                                          .Where(i => i.UserId == user.Id)
                                          .ToListAsync();

            if (!inventory.Any())
            {
                _logger.LogInformation($"No inventory found for user {userId}.");
                return NotFound(new
                {
                    success = false,
                    message = "No inventory items found.",
                    userId
                });
            }

            return Ok(new
            {
                success = true,
                message = "Inventory retrieved successfully.",
                totalItems = inventory.Count,
                inventory
            });
        }

        // ✅ ADD A NEW INVENTORY ITEM
        [HttpPost]
        public async Task<IActionResult> AddInventory([FromBody] Inventory inventoryItem)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized(new { success = false, message = "User not found or not logged in." });
            }

            if (inventoryItem == null)
            {
                return BadRequest(new { success = false, message = "Invalid inventory data." });
            }

            // ✅ Manually set UserId to the logged-in user's ID before validation
            inventoryItem.UserId = user.Id;

            if (string.IsNullOrWhiteSpace(inventoryItem.Name) || inventoryItem.Quantity <= 0 || inventoryItem.Price <= 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid inventory data. Ensure Name, Quantity, and Price are provided correctly."
                });
            }

            try
            {
                _context.Inventories.Add(inventoryItem);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetInventory), new { id = inventoryItem.Id },
                    new { success = true, message = "Inventory item added successfully.", inventory = inventoryItem });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while adding inventory." });
            }
        }

        // ✅ UPDATE AN EXISTING INVENTORY ITEM WITH FEEDBACK ON CHANGES
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateInventory(int id, [FromBody] Inventory updatedInventory)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized(new { success = false, message = "User not found or not logged in." });
            }

            var inventoryItem = await _context.Inventories.FindAsync(id);
            if (inventoryItem == null)
            {
                return NotFound(new { success = false, message = "Inventory item not found." });
            }

            if (inventoryItem.UserId != user.Id)
            {
                return Forbid();
            }

            // ✅ Assign UserId to logged-in user instead of requiring it in request body
            updatedInventory.UserId = user.Id;

            var changes = new List<string>();

            if (!string.IsNullOrWhiteSpace(updatedInventory.Name) && updatedInventory.Name != inventoryItem.Name)
            {
                changes.Add($"Name: '{inventoryItem.Name}' → '{updatedInventory.Name}'");
                inventoryItem.Name = updatedInventory.Name;
            }
            if (!string.IsNullOrWhiteSpace(updatedInventory.Description) && updatedInventory.Description != inventoryItem.Description)
            {
                changes.Add($"Description: '{inventoryItem.Description}' → '{updatedInventory.Description}'");
                inventoryItem.Description = updatedInventory.Description;
            }
            if (updatedInventory.Quantity > 0 && updatedInventory.Quantity != inventoryItem.Quantity)
            {
                changes.Add($"Quantity: {inventoryItem.Quantity} → {updatedInventory.Quantity}");
                inventoryItem.Quantity = updatedInventory.Quantity;
            }
            if (updatedInventory.Price > 0 && updatedInventory.Price != inventoryItem.Price)
            {
                changes.Add($"Price: {inventoryItem.Price} → {updatedInventory.Price}");
                inventoryItem.Price = updatedInventory.Price;
            }

            if (!changes.Any())
            {
                return BadRequest(new { success = false, message = "No changes detected." });
            }

            _context.Inventories.Update(inventoryItem);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Inventory item updated successfully.", changes, inventory = inventoryItem });
        }
        // ✅ DELETE AN INVENTORY ITEM
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInventory(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized(new { success = false, message = "User not found or not logged in." });
            }

            var inventoryItem = await _context.Inventories.FindAsync(id);
            if (inventoryItem == null)
            {
                return NotFound(new { success = false, message = "Inventory item not found." });
            }

            if (inventoryItem.UserId != user.Id)
            {
                return Forbid();
            }

            try
            {
                _context.Inventories.Remove(inventoryItem);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Inventory item deleted successfully.", deletedItemId = id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while deleting inventory." });
            }
        }

    }
}
