using AnilUniversity.Data;
using AnilUniversity.Models;
using AnilUniversity.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ======================================
// Add Services
// ======================================

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;

    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// Identity Cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

// Email Settings
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

builder.Services.AddTransient<IEmailService, EmailService>();

var app = builder.Build();

// ======================================
// Seed Roles & Admin
// ======================================

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await DbInitializer.SeedRolesAndAdminAsync(services);
}

// ======================================
// Configure Middleware
// ======================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseStatusCodePagesWithReExecute("/Home/StatusCode", "?code={0}");

app.UseAuthentication();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages();

// ======================================
// Rotativa PDF
// ======================================

Rotativa.AspNetCore.RotativaConfiguration.Setup(
    app.Environment.WebRootPath,
    "Rotativa");

// ======================================
// Run
// ======================================

app.Run();