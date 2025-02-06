using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace StockMaster.Models
{
    public class SmartStockDbContext : IdentityDbContext<IdentityUser>
    {
        public SmartStockDbContext(DbContextOptions<SmartStockDbContext> options)
            : base(options)
        {
        }

        public DbSet<Inventory> Inventories { get; set; } // Inventory table

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Ensure cascading delete is handled properly
            builder.Entity<Inventory>()
                .HasOne<IdentityUser>() // Link Inventory to IdentityUser
                .WithMany()
                .HasForeignKey(i => i.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
