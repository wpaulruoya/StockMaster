using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StockMaster.Models;  // ✅ Ensure this matches the correct namespace of SmartStockDbContext
using System;
using System.Linq;
using System.Threading.Tasks;

public class DbInitializer
{
    private readonly SmartStockDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public DbInitializer(SmartStockDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task InitializeAsync()
    {
        // Apply pending migrations
        if (_context.Database.GetPendingMigrations().Any())
        {
            await _context.Database.MigrateAsync();
        }

        // Create default roles if they don't exist
        string[] roleNames = { "SuperAdmin", "Admin", "User", "Manager" };
        foreach (var roleName in roleNames)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // Create Super Admin user
        var adminEmail = "superadmin@example.com";
        var adminUser = await _userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new IdentityUser
            {
                UserName = "superadmin",
                Email = adminEmail,
                EmailConfirmed = true
            };

            var createUserResult = await _userManager.CreateAsync(adminUser, "SuperAdmin@123");
            if (createUserResult.Succeeded)
            {
                await _userManager.AddToRoleAsync(adminUser, "SuperAdmin");
                Console.WriteLine("SuperAdmin user created successfully.");
            }
            else
            {
                Console.WriteLine("Error creating SuperAdmin user:");
                foreach (var error in createUserResult.Errors)
                {
                    Console.WriteLine($" - {error.Description}");
                }
            }
        }
        else
        {
            Console.WriteLine("SuperAdmin user already exists.");
        }
    }
}
