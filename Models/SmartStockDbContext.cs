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

        // ✅ Add Inventory Table
        public DbSet<Inventory> Inventories { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ✅ Set up foreign key relationship with Identity Users table
            builder.Entity<Inventory>()
                .HasOne<IdentityUser>()
                .WithMany()
                .HasForeignKey(i => i.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
