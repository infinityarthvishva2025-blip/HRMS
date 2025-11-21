using HRMS.Data;
using HRMS.Services;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;          // EPPlus
using Rotativa.AspNetCore;    // Rotativa for PDF

var builder = WebApplication.CreateBuilder(args);

// =================== EPPLUS LICENSE ===================
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

// =================== GET HOST ENVIRONMENT ===================
var env = builder.Environment;

// =================== ROTATIVA CONFIG ===================
// Rotativa needs the *physical path* to wwwroot and the Rotativa folder name
//RotativaConfiguration.Setup(env.WebRootPath, "Rotativa");

// =================== SERVICES ===================
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IWorkflowService, WorkflowService>();
builder.Services.AddScoped<INotificationService, EmailNotificationService>();

// SESSION
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Razor runtime compilation (auto refresh views)
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();

var app = builder.Build();

// =================== MIDDLEWARE ===================
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// SESSION must be BEFORE Authorization
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// =================== ROUTING ===================
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
