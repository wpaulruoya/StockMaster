using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;
using StockMaster.Interfaces;
using StockMaster.Models;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace StockMaster.Controllers.API
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/inventory")]
    [ApiController]
    public class InventoryApiController : ControllerBase
    {
        private readonly IApiInventoryService _apiInventoryService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<InventoryApiController> _logger;

        public InventoryApiController(IApiInventoryService apiInventoryService, UserManager<IdentityUser> userManager, ILogger<InventoryApiController> logger)
        {
            _apiInventoryService = apiInventoryService;
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
        public async Task<IActionResult> GetInventory()
        {
            var userId = GetUserIdFromToken();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { success = false, message = "Invalid token or user not found." });

            var inventory = await _apiInventoryService.GetInventoryByUserId(userId);
            if (inventory == null || inventory.Count == 0)
                return NotFound(new { success = false, message = "No inventory items found." });

            return Ok(new { success = true, message = "Inventory retrieved successfully.", totalItems = inventory.Count, inventory });
        }

        [HttpPost]
        public async Task<IActionResult> AddInventory([FromBody] Inventory inventoryItem)
        {
            var userId = GetUserIdFromToken();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { success = false, message = "Invalid token or user not found." });

            if (inventoryItem == null || string.IsNullOrWhiteSpace(inventoryItem.Name) || inventoryItem.Quantity <= 0 || inventoryItem.Price <= 0)
                return BadRequest(new { success = false, message = "Invalid inventory data." });

            var addedInventory = await _apiInventoryService.AddInventory(userId, inventoryItem);
            return CreatedAtAction(nameof(GetInventory), new { id = addedInventory.Id },
                new { success = true, message = "Inventory item added successfully.", inventory = addedInventory });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateInventory(int id, [FromBody] Inventory updatedInventory)
        {
            var userId = GetUserIdFromToken();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { success = false, message = "Invalid token or user not found." });

            var inventoryItem = await _apiInventoryService.UpdateInventory(userId, id, updatedInventory);
            if (inventoryItem == null)
                return NotFound(new { success = false, message = "Inventory item not found or not owned by user." });

            return Ok(new { success = true, message = "Inventory item updated successfully.", inventory = inventoryItem });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInventory(int id)
        {
            var userId = GetUserIdFromToken();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { success = false, message = "Invalid token or user not found." });

            bool isDeleted = await _apiInventoryService.DeleteInventory(userId, id);
            if (!isDeleted)
                return NotFound(new { success = false, message = "Inventory item not found or not owned by user." });

            return Ok(new { success = true, message = "Inventory item deleted successfully." });
        }
    }
}
