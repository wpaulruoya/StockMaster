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
    }
}
