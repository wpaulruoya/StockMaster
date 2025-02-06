using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace StockMaster.Models
{
    public class SmartStockDbContext : IdentityDbContext<IdentityUser>
    {
        public SmartStockDbContext(DbContextOptions<SmartStockDbContext> options) : base(options)
        {
        }

        public DbSet<Inventory> Inventories { get; set; } // Inventory table
    }
}
