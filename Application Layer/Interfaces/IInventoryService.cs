using StockMaster.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IInventoryService
{
    Task<List<Inventory>> GetUserInventoryAsync(string userId);
    Task<Inventory> GetInventoryByIdAsync(int id, string userId);
    Task<bool> AddInventoryAsync(Inventory inventory);
    Task<bool> UpdateInventoryAsync(Inventory inventory);
    Task<bool> DeleteInventoryAsync(int id, string userId);
}
