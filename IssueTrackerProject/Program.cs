using IssueTrackerProject.Models;
using Microsoft.EntityFrameworkCore;
using IssueTrackerProject.Data;
using Microsoft.AspNetCore.Identity; // Required for RoleManager

var builder = WebApplication.CreateBuilder(args);

// 1. SERVICES SETUP
builder.Services.AddControllersWithViews();

// Register MySQL Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), 
    new MySqlServerVersion(new Version(8, 0, 0))));

// --- ADD IDENTITY SERVICES HERE ---
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>() // This allows the use of Roles
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddRazorPages(); 

var app = builder.Build();

// 2. PIPELINE SETUP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); 

app.UseRouting();

// Authentication MUST come before Authorization
app.UseAuthentication(); 
app.UseAuthorization();

// 3. ROUTING SETUP
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Issue}/{action=Index}/{id?}");
app.MapRazorPages();

// --- ROLE SEEDING LOGIC ---
// This runs every time the app starts to ensure roles exist in MySQL
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    // 1. Seed Roles (Already done, but keep for context)
    string[] roles = { "QA Tester", "Backend Dev", "Admin" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }
        var adminEmail = "admin@test.com";
    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
        var adminUser = new IdentityUser { 
            UserName = adminEmail, 
            Email = adminEmail, 
            EmailConfirmed = true 
        };
        // Password must meet your requirements (e.g., length, uppercase, etc.)
        await userManager.CreateAsync(adminUser, "Admin123!"); 
        await userManager.AddToRoleAsync(adminUser, "Admin");
    }
    // 2. Seed a Sample QA Tester
    var qaEmail = "qa@test.com";
    if (await userManager.FindByEmailAsync(qaEmail) == null)
    {
        var qaUser = new IdentityUser { UserName = qaEmail, Email = qaEmail, EmailConfirmed = true };
        await userManager.CreateAsync(qaUser, "Password123!"); // Default password
        await userManager.AddToRoleAsync(qaUser, "QA Tester");
    }

    // 3. Seed a Sample Backend Developer
    var devEmail = "dev@test.com";
    if (await userManager.FindByEmailAsync(devEmail) == null)
    {
        var devUser = new IdentityUser { UserName = devEmail, Email = devEmail, EmailConfirmed = true };
        await userManager.CreateAsync(devUser, "Password123!");
        await userManager.AddToRoleAsync(devUser, "Backend Dev");
    }
}

app.Run();