using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using StockMaster.Models;
using StockMaster.ApplicationLayer.Interfaces;
using StockMaster.ApplicationLayer.Services;
using StockMaster.Interfaces;
using StockMaster.Services;

var builder = WebApplication.CreateBuilder(args);

// ✅ Load Configuration
builder.Configuration.SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "Presentation Layer"))
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// ✅ Configure Database Context
builder.Services.AddDbContext<SmartStockDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ✅ Configure Identity with Roles & Allow Spaces in Usernames
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 ";
})
.AddEntityFrameworkStores<SmartStockDbContext>()
.AddDefaultTokenProviders();

// ✅ Configure Authentication (Only Cookies for MVC)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.Cookie.HttpOnly = true;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    });

// ✅ Enable Sessions
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ✅ Add Controllers and Views
builder.Services.AddControllersWithViews()
    .AddRazorOptions(options =>
    {
        options.ViewLocationFormats.Clear();
        options.ViewLocationFormats.Add("/Presentation Layer/Views/{1}/{0}.cshtml");
        options.ViewLocationFormats.Add("/Presentation Layer/Views/Shared/{0}.cshtml");
    });

// ✅ Register Services (MVC)
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();

// ✅ Build Application


var app = builder.Build();

// ✅ Use HTTP only in Docker
if (app.Environment.IsDevelopment() || Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true")
{
    app.Urls.Add("http://*:5120"); // ✅ Ensure HTTP works inside Docker
}
else
{
    app.Urls.Add("http://*:5120");
    app.Urls.Add("https://*:7085");
}

// ✅ Middleware Pipeline (Fix Order)
//app.UseHttpsRedirection(); // 🔄 Redirect HTTP → HTTPS
app.UseHsts();             // 🔐 Enforce HTTPS
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();


// ✅ Map Controllers & Routes
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

// ✅ Run Application
app.Run();
