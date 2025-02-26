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

        // ✅ UPDATE AN EXISTING INVENTORY ITEM
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

            // ✅ Apply updates
            inventoryItem.Name = updatedInventory.Name ?? inventoryItem.Name;
            inventoryItem.Description = updatedInventory.Description ?? inventoryItem.Description;
            inventoryItem.Quantity = updatedInventory.Quantity > 0 ? updatedInventory.Quantity : inventoryItem.Quantity;
            inventoryItem.Price = updatedInventory.Price > 0 ? updatedInventory.Price : inventoryItem.Price;

            _context.Inventories.Update(inventoryItem);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Inventory item updated successfully.", inventory = inventoryItem });
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

            _context.Inventories.Remove(inventoryItem);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Inventory item deleted successfully.", deletedItemId = id });
        }
    }
}
