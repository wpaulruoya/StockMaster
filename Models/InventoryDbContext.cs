using Microsoft.EntityFrameworkCore;
using StockMaster.Models;
using Microsoft.AspNetCore.Identity;

public class InventoryDbContext : DbContext
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options)
        : base(options)
    {
    }

    public DbSet<Inventory> Inventories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Ensure UserId Foreign Key does not use Cascade Delete
        modelBuilder.Entity<Inventory>()
            .HasOne<IdentityUser>() // Assuming IdentityUser is used for users
            .WithMany()
            .HasForeignKey(i => i.UserId)
            .OnDelete(DeleteBehavior.NoAction); // ❌ Prevent cascade delete
    }
}
