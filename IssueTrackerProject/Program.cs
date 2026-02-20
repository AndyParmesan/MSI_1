using IssueTrackerProject.Models;
using Microsoft.EntityFrameworkCore;
using IssueTrackerProject.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. SERVICES SETUP
builder.Services.AddControllersWithViews();

// Register MySQL Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), 
    new MySqlServerVersion(new Version(8, 0, 0))));

var app = builder.Build();

// 2. PIPELINE SETUP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// --- CRITICAL FIX: This allows Bootstrap and CSS to load ---
app.UseStaticFiles(); 

app.UseRouting();
app.UseAuthorization();

// 3. ROUTING SETUP
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Issue}/{action=Index}/{id?}");

app.Run();