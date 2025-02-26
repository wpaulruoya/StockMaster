using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StockMaster.Models;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace StockMaster.Controllers.Api
{
    [Authorize] // ✅ Ensure JWT authentication is required
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

        // ✅ Get the logged-in user's ID from JWT token
        private string GetUserIdFromToken()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        // ✅ VIEW INVENTORY ITEMS FOR LOGGED-IN USER
        [HttpGet]
        public async Task<ActionResult<object>> GetInventory()
        {
            var userId = GetUserIdFromToken();
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Unauthorized access attempt.");
                return Unauthorized(new { success = false, message = "Invalid token or user not found." });
            }

            _logger.LogInformation($"Fetching inventory for user ID: {userId}");

            var inventory = await _context.Inventories
                                          .Where(i => i.UserId == userId)
                                          .ToListAsync();

            if (!inventory.Any())
            {
                return NotFound(new { success = false, message = "No inventory items found.", userId });
            }

            return Ok(new { success = true, message = "Inventory retrieved successfully.", totalItems = inventory.Count, inventory });
        }

        // ✅ ADD A NEW INVENTORY ITEM
        [HttpPost]
        public async Task<IActionResult> AddInventory([FromBody] Inventory inventoryItem)
        {
            var userId = GetUserIdFromToken();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { success = false, message = "Invalid token or user not found." });
            }

            if (inventoryItem == null || string.IsNullOrWhiteSpace(inventoryItem.Name) || inventoryItem.Quantity <= 0 || inventoryItem.Price <= 0)
            {
                return BadRequest(new { success = false, message = "Invalid inventory data." });
            }

            // ✅ Assign UserId from token
            inventoryItem.UserId = userId;

            _context.Inventories.Add(inventoryItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetInventory), new { id = inventoryItem.Id },
                new { success = true, message = "Inventory item added successfully.", inventory = inventoryItem });
        }

        // ✅ Changing A NEW INVENTORY ITEM


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateInventory(int id, [FromBody] Inventory updatedInventory)
        {
            var userId = GetUserIdFromToken();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { success = false, message = "Invalid token or user not found." });
            }

            var inventoryItem = await _context.Inventories.FindAsync(id);
            if (inventoryItem == null || inventoryItem.UserId != userId)
            {
                return NotFound(new { success = false, message = "Inventory item not found or not owned by user." });
            }

            // Store original values before update
            var changes = new List<object>();

            if (!string.IsNullOrEmpty(updatedInventory.Name) && updatedInventory.Name != inventoryItem.Name)
            {
                changes.Add(new { Field = "Name", OldValue = inventoryItem.Name, NewValue = updatedInventory.Name });
                inventoryItem.Name = updatedInventory.Name;
            }

            if (!string.IsNullOrEmpty(updatedInventory.Description) && updatedInventory.Description != inventoryItem.Description)
            {
                changes.Add(new { Field = "Description", OldValue = inventoryItem.Description, NewValue = updatedInventory.Description });
                inventoryItem.Description = updatedInventory.Description;
            }

            if (updatedInventory.Quantity > 0 && updatedInventory.Quantity != inventoryItem.Quantity)
            {
                changes.Add(new { Field = "Quantity", OldValue = inventoryItem.Quantity, NewValue = updatedInventory.Quantity });
                inventoryItem.Quantity = updatedInventory.Quantity;
            }

            if (updatedInventory.Price > 0 && updatedInventory.Price != inventoryItem.Price)
            {
                changes.Add(new { Field = "Price", OldValue = inventoryItem.Price, NewValue = updatedInventory.Price });
                inventoryItem.Price = updatedInventory.Price;
            }

            if (!changes.Any())
            {
                return Ok(new { success = false, message = "No changes detected.", inventory = inventoryItem });
            }

            _context.Inventories.Update(inventoryItem);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Inventory item updated successfully.",
                changes = changes,
                inventory = inventoryItem
            });
        }

        // ✅ DELETE AN INVENTORY ITEM
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInventory(int id)
        {
            var userId = GetUserIdFromToken();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { success = false, message = "Invalid token or user not found." });
            }

            var inventoryItem = await _context.Inventories.FindAsync(id);
            if (inventoryItem == null || inventoryItem.UserId != userId)
            {
                return NotFound(new { success = false, message = "Inventory item not found or not owned by user." });
            }

            // Store item details before deletion
            var deletedItemDetails = new
            {
                Id = inventoryItem.Id,
                Name = inventoryItem.Name,
                Description = inventoryItem.Description
            };

            _context.Inventories.Remove(inventoryItem);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Inventory item deleted successfully.",
                deletedItem = deletedItemDetails
            });
        }

    }
}
