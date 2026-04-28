using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using QDPhone.Web.Data;
using QDPhone.Web.Data.Seed;
using QDPhone.Web.Models.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (builder.Environment.IsEnvironment("Testing"))
    {
        options.UseInMemoryDatabase("QDPhoneTestDb");
    }
    else
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    }
});

builder.Services
    .AddIdentity<AppUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedEmail = false;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 8;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("StaffOrAdmin", policy => policy.RequireRole("Admin", "Staff"));
});

builder.Services.AddMemoryCache();
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());
});

var app = builder.Build();

if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var provider = scope.ServiceProvider;
    var context = provider.GetRequiredService<ApplicationDbContext>();
    var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = provider.GetRequiredService<UserManager<AppUser>>();
    try
    {
        await context.Database.MigrateAsync();
        await DbSeeder.SeedRolesAndAdminAsync(roleManager, userManager);
    }
    catch (SqlException ex) when (IsLikelyServerUnreachable(ex))
    {
        var cs = builder.Configuration.GetConnectionString("DefaultConnection");
        Console.Error.WriteLine();
        Console.Error.WriteLine("=== QDPhone Base: KHÔNG KẾT NỐI ĐƯỢC SQL SERVER ===");
        Console.Error.WriteLine("Mã lỗi: " + ex.Number + " — " + ex.Message);
        if (!string.IsNullOrWhiteSpace(cs) && cs.Contains("Server=", StringComparison.OrdinalIgnoreCase))
        {
            var serverPart = cs.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .FirstOrDefault(s => s.StartsWith("Server=", StringComparison.OrdinalIgnoreCase));
            if (serverPart is not null)
                Console.Error.WriteLine("Server trong cấu hình: " + serverPart);
        }
        Console.Error.WriteLine();
        Console.Error.WriteLine("Cách xử lý (chọn một):");
        Console.Error.WriteLine("  1) Docker: docker compose up -d sqlserver");
        Console.Error.WriteLine("  2) Sửa ConnectionStrings:DefaultConnection trong appsettings.Development.json");
        Console.Error.WriteLine("==============================================");
        Console.Error.WriteLine();
        Environment.Exit(1);
    }
}

static bool IsLikelyServerUnreachable(SqlException ex)
    => ex.Number is 2 or 53 or -2 or 10053 or 10060 or 10061;

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "SAMEORIGIN";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    await next();
});
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

public partial class Program;
