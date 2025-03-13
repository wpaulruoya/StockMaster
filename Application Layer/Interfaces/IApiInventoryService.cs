using System.Collections.Generic;
using System.Threading.Tasks;
using StockMaster.Models;

namespace StockMaster.Interfaces
{
    public interface IApiInventoryService
    {
        Task<List<Inventory>> GetInventoryByUserId(string userId);
        Task<Inventory> AddInventory(string userId, Inventory inventory);
        Task<Inventory> UpdateInventory(string userId, int id, Inventory updatedInventory);
        Task<bool> DeleteInventory(string userId, int id);
    }
}
