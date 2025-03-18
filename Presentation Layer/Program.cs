using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Tokens;
using StockMaster.Models;
using System.Text;
using StockMaster.ApplicationLayer.Interfaces;
using StockMaster.ApplicationLayer.Services;
using StockMaster.Controllers.API;
using StockMaster.Application_Layer.Interfaces;
using StockMaster.Application_Layer.Services;
using StockMaster.Interfaces;
using StockMaster.Services;

var builder = WebApplication.CreateBuilder(args);

// ✅ Load Configuration
builder.Configuration.SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "Presentation Layer"))
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// ✅ Configure Database Context
builder.Services.AddDbContext<SmartStockDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ✅ Configure Identity with Roles
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<SmartStockDbContext>()
    .AddDefaultTokenProviders();

// ✅ Configure Identity options to allow spaces in usernames
builder.Services.Configure<IdentityOptions>(options =>
{
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 -._@+";
    options.User.RequireUniqueEmail = true;
});

// ✅ Read JWT settings
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var jwtKey = jwtSettings["Key"];
var jwtIssuer = jwtSettings["Issuer"];
var jwtAudience = jwtSettings["Audience"];

if (string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience))
{
    throw new InvalidOperationException("JWT configuration is missing in appsettings.json");
}

// ✅ Configure Hybrid Authentication (Cookies for MVC, JWT for API)
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
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

// ✅ Allow API calls without Cookie authentication
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = 401;
            return Task.CompletedTask;
        }
        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
});

// ✅ Enable CORS for API & Frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.WithOrigins("http://localhost:3000")
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

// ✅ Add Controllers and MVC Views
builder.Services.AddControllersWithViews()
    .AddRazorOptions(options =>
    {
        options.ViewLocationFormats.Clear();
        options.ViewLocationFormats.Add("/Presentation Layer/Views/{1}/{0}.cshtml");
        options.ViewLocationFormats.Add("/Presentation Layer/Views/Shared/{0}.cshtml");
    });
builder.Services.AddControllers();

// ✅ Register Services (MVC)
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();

// ✅ Register API Services (JWT-based)
builder.Services.AddScoped<IApiUserService, ApiUserService>();
builder.Services.AddScoped<IApiInventoryService, ApiInventoryService>();

// ✅ Configure Swagger with JWT Support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter 'Bearer {token}' in the field below."
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// ✅ Build Application
var app = builder.Build();

// ✅ Apply Database Migrations Automatically
using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    var dbContext = serviceProvider.GetRequiredService<SmartStockDbContext>();
    dbContext.Database.Migrate(); // Ensure database is up to date

    // Create roles and SuperAdmin user
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

    // Ensure roles exist
    string[] roles = new[] { "SuperAdmin", "Admin", "User" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }


    // Ensure SuperAdmin exists
    string superAdminEmail = "stockmaster@gmail.com";
    string superAdminPassword = "Admin@2025";
    var superAdmin = await userManager.FindByEmailAsync(superAdminEmail);

    if (superAdmin == null)
    {
        superAdmin = new IdentityUser { UserName = superAdminEmail, Email = superAdminEmail, EmailConfirmed = true };
        var result = await userManager.CreateAsync(superAdmin, superAdminPassword);

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(superAdmin, "SuperAdmin");
        }
    }
}

// ✅ Middleware Pipeline
app.UseExceptionHandler("/Home/Error");
app.UseHsts();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.UseCors("AllowAll");

// ✅ Swagger Setup
app.UseSwagger();
app.UseSwaggerUI();

// ✅ Map Controllers & Routes
app.MapControllers();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

// ✅ Run Application
app.Run();
