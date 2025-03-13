using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StockMaster.Models; // ✅ Ensure correct namespace after merging
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace StockMaster.Controllers.API // ✅ Updated namespace after merge
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/inventory")] // ✅ More explicit route
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

        private string GetUserIdFromToken()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("🚨 User ID not found in token.");
            }
            return userId;
        }

        [HttpGet]
        public async Task<ActionResult<object>> GetInventory()
        {
            var userId = GetUserIdFromToken();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { success = false, message = "Invalid token or user not found." });
            }

            var inventory = await _context.Inventories
                .Where(i => i.UserId == userId)
                .ToListAsync();

            if (!inventory.Any())
            {
                return NotFound(new { success = false, message = "No inventory items found." });
            }

            return Ok(new { success = true, message = "Inventory retrieved successfully.", totalItems = inventory.Count, inventory });
        }

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

            inventoryItem.UserId = userId;
            _context.Inventories.Add(inventoryItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetInventory), new { id = inventoryItem.Id },
                new { success = true, message = "Inventory item added successfully.", inventory = inventoryItem });
        }

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

            inventoryItem.Name = updatedInventory.Name ?? inventoryItem.Name;
            inventoryItem.Description = updatedInventory.Description ?? inventoryItem.Description;
            inventoryItem.Quantity = updatedInventory.Quantity > 0 ? updatedInventory.Quantity : inventoryItem.Quantity;
            inventoryItem.Price = updatedInventory.Price > 0 ? updatedInventory.Price : inventoryItem.Price;

            _context.Inventories.Update(inventoryItem);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Inventory item updated successfully.", inventory = inventoryItem });
        }

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

            return Ok(new { success = true, message = "Inventory item deleted successfully.", deletedItem = inventoryItem });
        }
    }
}
