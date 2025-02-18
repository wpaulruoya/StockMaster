using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StockMaster.Models;
using Microsoft.OpenApi.Models;


var builder = WebApplication.CreateBuilder(args);

// ✅ Configure SmartStockDbContext
builder.Services.AddDbContext<SmartStockDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ✅ Configure Identity with relaxed password rules
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
    .AddEntityFrameworkStores<SmartStockDbContext>();

// ✅ Add API Controllers and MVC Controllers separately
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = null; // Keep property names unchanged
});
builder.Services.AddControllersWithViews();

// ✅ Enable CORS (for API access from frontend applications)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader());
});

// ✅ Add Sessions
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ✅ Add Swagger (for API documentation)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

// ✅ Enable CORS (Must be before authentication)
app.UseCors("AllowAll");

// ✅ Enable Sessions (Must be before authentication)
app.UseSession();

// ✅ Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// ✅ Enable Swagger in Development Mode
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ✅ Map API Controllers Correctly
app.MapControllers(); // Ensures API controllers are handled properly

// ✅ Map MVC Controllers
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ✅ Log registered routes
var logger = app.Services.GetRequiredService<ILogger<Program>>();
foreach (var endpoint in app.Services.GetRequiredService<Microsoft.AspNetCore.Routing.EndpointDataSource>().Endpoints)
{
    logger.LogInformation($"Registered Endpoint: {endpoint.DisplayName}");
}

// ✅ Start the app
app.Run();
