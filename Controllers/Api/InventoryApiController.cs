// InventoryApiController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StockMaster.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StockMaster.Controllers.Api
{
    [Authorize]
    [Route("api/inventory")]
    [ApiController]
    public class InventoryApiController : ControllerBase
    {
        private readonly SmartStockDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public InventoryApiController(SmartStockDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ✅ VIEW INVENTORY ITEMS FOR LOGGED-IN USER
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Inventory>>> GetInventory()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var inventory = await _context.Inventories
                                          .Where(i => i.UserId == user.Id)
                                          .ToListAsync();

            return Ok(inventory);
        }
    }
}
