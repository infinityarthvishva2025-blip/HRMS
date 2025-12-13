
using HRMS.Data;
using HRMS.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using OfficeOpenXml;

var builder = WebApplication.CreateBuilder(args);

// EPPLUS
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

var env = builder.Environment;

// SERVICES
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// REGISTER IHttpContextAccessor  ✅ REQUIRED
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IWorkflowService, WorkflowService>();
builder.Services.AddScoped<INotificationService, EmailNotificationService>();

// SESSION
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;

    // 🔥 ADD THESE TWO LINES
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});


// Razor auto-refresh
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();

var app = builder.Build();

// MIDDLEWARE
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// SESSION BEFORE AUTH
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

//app.UseStaticFiles(new StaticFileOptions
//{
//    FileProvider = new PhysicalFileProvider(@"C:\HRMSFiles"),
//    RequestPath = "/HRMSFiles"
//});

// ROUTING
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();



