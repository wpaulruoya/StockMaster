using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StockMaster.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ✅ Load appsettings.json correctly
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

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
    .AddRoles<IdentityRole>() // ✅ Enable Roles
    .AddEntityFrameworkStores<SmartStockDbContext>();

// ✅ Read JWT settings correctly with Debugging
var jwtSettings = builder.Configuration.GetSection("JwtSettings");

var jwtKey = jwtSettings["Key"];
var jwtIssuer = jwtSettings["Issuer"];
var jwtAudience = jwtSettings["Audience"];

// ✅ Debug Log to Check if Values Are Loaded Correctly
Console.WriteLine($"[DEBUG] JWT Key: {jwtKey}");
Console.WriteLine($"[DEBUG] JWT Issuer: {jwtIssuer}");
Console.WriteLine($"[DEBUG] JWT Audience: {jwtAudience}");

if (string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience))
{
    throw new InvalidOperationException("JWT configuration is missing in appsettings.json");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        ClockSkew = TimeSpan.Zero
    };
});

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

// ✅ Enable Sessions
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ✅ Add Swagger with JWT Support (Always Enabled)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer {token}'"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// ✅ Middleware Pipeline
app.UseExceptionHandler("/Home/Error");
app.UseHsts();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// ✅ Enable CORS
app.UseCors("AllowAll");

// ✅ Enable Sessions
app.UseSession();

// ✅ Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// ✅ Prevent Back Navigation After Logout
app.Use(async (context, next) =>
{
    context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
    context.Response.Headers["Pragma"] = "no-cache";
    context.Response.Headers["Expires"] = "-1";
    await next();
});

// ✅ Enable Swagger in All Environments
app.UseSwagger();
app.UseSwaggerUI();

// ✅ Create Roles and Super Admin at Startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await CreateRolesAndSuperAdmin(services);
}

// ✅ Map API Controllers Correctly
app.MapControllers();

// ✅ Map MVC Controllers
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ✅ Start the app
app.Run();

// =======================================================
// ✅ Function to Create Roles & Super Admin at Startup
// =======================================================
async Task CreateRolesAndSuperAdmin(IServiceProvider serviceProvider)
{
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

    string[] roleNames = { "SuperAdmin", "Admin", "User" };

    foreach (var role in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // ✅ Super Admin credentials
    string adminEmail = "StockMaster@gmail.com";
    string adminPassword = "Admin@2025";

    var superAdmin = await userManager.FindByEmailAsync(adminEmail);
    if (superAdmin == null)
    {
        var user = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
        var result = await userManager.CreateAsync(user, adminPassword);

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, "SuperAdmin");
        }
    }
    else
    {
        // ✅ Reset password for Super Admin
        await userManager.RemovePasswordAsync(superAdmin);
        await userManager.AddPasswordAsync(superAdmin, adminPassword);
    }
}
