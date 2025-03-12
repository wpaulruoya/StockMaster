using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StockMaster.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class InventoryService : IInventoryService
{
    private readonly SmartStockDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public InventoryService(SmartStockDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<List<Inventory>> GetUserInventoryAsync(string userId)
    {
        return await _context.Inventories.Where(i => i.UserId == userId).ToListAsync();
    }

    public async Task<Inventory> GetInventoryByIdAsync(int id, string userId)
    {
        return await _context.Inventories.FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);
    }

    public async Task<bool> AddInventoryAsync(Inventory inventory)
    {
        try
        {
            _context.Inventories.Add(inventory);
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> UpdateInventoryAsync(Inventory inventory)
    {
        try
        {
            _context.Inventories.Update(inventory);
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeleteInventoryAsync(int id, string userId)
    {
        var inventory = await GetInventoryByIdAsync(id, userId);
        if (inventory == null) return false;

        try
        {
            _context.Inventories.Remove(inventory);
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }
}
