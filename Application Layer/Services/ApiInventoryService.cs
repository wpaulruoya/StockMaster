using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StockMaster.Interfaces;
using StockMaster.Models;

namespace StockMaster.Services
{
    public class ApiInventoryService : IApiInventoryService
    {
        private readonly SmartStockDbContext _context;
        private readonly ILogger<ApiInventoryService> _logger;

        public ApiInventoryService(SmartStockDbContext context, ILogger<ApiInventoryService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Inventory>> GetInventoryByUserId(string userId)
        {
            return await _context.Inventories.Where(i => i.UserId == userId).ToListAsync();
        }

        public async Task<Inventory> AddInventory(string userId, Inventory inventory)
        {
            inventory.UserId = userId;
            _context.Inventories.Add(inventory);
            await _context.SaveChangesAsync();
            return inventory;
        }

        public async Task<Inventory> UpdateInventory(string userId, int id, Inventory updatedInventory)
        {
            var inventoryItem = await _context.Inventories.FindAsync(id);
            if (inventoryItem == null || inventoryItem.UserId != userId)
                return null;

            inventoryItem.Name = updatedInventory.Name ?? inventoryItem.Name;
            inventoryItem.Description = updatedInventory.Description ?? inventoryItem.Description;
            inventoryItem.Quantity = updatedInventory.Quantity > 0 ? updatedInventory.Quantity : inventoryItem.Quantity;
            inventoryItem.Price = updatedInventory.Price > 0 ? updatedInventory.Price : inventoryItem.Price;

            _context.Inventories.Update(inventoryItem);
            await _context.SaveChangesAsync();
            return inventoryItem;
        }

        public async Task<bool> DeleteInventory(string userId, int id)
        {
            var inventoryItem = await _context.Inventories.FindAsync(id);
            if (inventoryItem == null || inventoryItem.UserId != userId)
                return false;

            _context.Inventories.Remove(inventoryItem);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
