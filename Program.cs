using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using StockMaster.Models;

var builder = WebApplication.CreateBuilder(args);

// ✅ Configure SmartStockDbContext (Only One Context for Users & Inventory)
builder.Services.AddDbContext<SmartStockDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ✅ Configure Identity
builder.Services.AddDefaultIdentity<IdentityUser>()
    .AddEntityFrameworkStores<SmartStockDbContext>();

// ✅ Add Controllers & Views
builder.Services.AddControllersWithViews();

// ✅ Add API Controllers (New)
builder.Services.AddControllers();

// ✅ Add Sessions
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// ✅ Middleware Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// ✅ Enable Sessions
app.UseSession();

// ✅ Map MVC Controllers
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ✅ Map API Controllers (New)
app.MapControllers();

app.Run();
