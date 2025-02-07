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

        public DbSet<Inventory> Inventories { get; set; } 

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Inventory>()
                .Property(i => i.UserId)
                .HasColumnName("UserId"); // ✅ Explicitly set the column name

            builder.Entity<Inventory>()
                .HasOne(i => i.User)
                .WithMany()
                .HasForeignKey(i => i.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }

    }
}
