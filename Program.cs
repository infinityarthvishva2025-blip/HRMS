
using HRMS.Data;
using HRMS.Jobs;
using HRMS.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using OfficeOpenXml;
using Quartz;
using Rotativa.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// EPPLUS
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;


var env = builder.Environment;
// SERVICES
builder.Services.AddControllersWithViews();

// DB CONTEXT
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// REGISTER IHttpContextAccessor  ✅ REQUIRED
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IWorkflowService, WorkflowService>();
builder.Services.AddScoped<INotificationService, EmailNotificationService>();

builder.Services.AddScoped<PayslipEmailService>();
builder.Services.AddScoped<IPayrollService, PayrollService>();


// SESSION
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;


    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

// Razor auto-refresh
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();

// AUTH
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

builder.Services.AddAuthorization();

// ===============================
// 🔥 QUARTZ CONFIGURATION (AUTO)
// ===============================
builder.Services.AddQuartz(q =>
{
    var jobKey = new JobKey("MonthlyAttendanceJob");

    q.AddJob<MonthlyAttendanceJob>(opts =>
        opts.WithIdentity(jobKey)
    );

    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("MonthlyAttendanceTrigger")
       // Runs EVERY MONTH on 25th at 02:00 AM
        .WithCronSchedule("0 0 2 25 * ?")
       //.WithCronSchedule("0 */1 * * * ?")
    );
});

// 🔹 Quartz hosted service
builder.Services.AddQuartzHostedService(q =>
{
    q.WaitForJobsToComplete = true;
});

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



app.UseRotativa();
// ROUTING
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();



