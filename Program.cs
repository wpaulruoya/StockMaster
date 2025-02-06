using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StockMaster.Models;

var builder = WebApplication.CreateBuilder(args);

// 🔹 Configure SQL Server Database
builder.Services.AddDbContext<SmartStockDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 🔹 Add Identity with EF Core
builder.Services.AddDefaultIdentity<IdentityUser>()
    .AddEntityFrameworkStores<SmartStockDbContext>();

// 🔹 Add Controllers & Views
builder.Services.AddControllersWithViews();

var app = builder.Build();

// 🔹 Middleware Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication(); // Ensure authentication is enabled
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
