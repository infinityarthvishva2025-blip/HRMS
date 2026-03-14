using HRMS.Data;
using HRMS.Jobs;
using HRMS.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using OfficeOpenXml;
using Quartz;
using Rotativa.AspNetCore;
using Hangfire;
using Hangfire.SqlServer;
using QuestPDF.Infrastructure;
var builder = WebApplication.CreateBuilder(args);

// EPPLUS
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

var env = builder.Environment;

// SERVICES
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<ResignationService>();

// DATABASE
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// REGISTER IHttpContextAccessor
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IWorkflowService, WorkflowService>();
builder.Services.AddScoped<INotificationService, EmailNotificationService>();

builder.Services.AddScoped<PayslipEmailService>();
builder.Services.AddScoped<PayrollService>();

builder.Services.AddScoped<ICompOffService, CompOffService>();
builder.Services.AddHostedService<CompOffExpiryHostedService>();
builder.Services.AddScoped<ExperiencePdfService>();
// ❌ REMOVE OLD BACKGROUND SERVICE
// builder.Services.AddHostedService<RelievingLetterJob>();

// SMTP
builder.Services.Configure<SmtpSettings>(
    builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddHttpClient<PanVerificationService>();
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddScoped<IPdfService, QuestPdfService>();
builder.Services.AddHttpClient();
// QuestPDF License
QuestPDF.Settings.License = LicenseType.Community;
// ===============================
// 🔥 HANGFIRE CONFIGURATION
// ===============================
builder.Services.AddHangfire(config =>
    config.UseSqlServerStorage(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new SqlServerStorageOptions
        {
            CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
            SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
            QueuePollInterval = TimeSpan.FromSeconds(15),
            UseRecommendedIsolationLevel = true,
            DisableGlobalLocks = true
        }));

builder.Services.AddHangfireServer();

// Register job
builder.Services.AddScoped<RelievingLetterHangfireJob>();

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
// 🔥 QUARTZ CONFIGURATION
// ===============================
builder.Services.AddQuartz(q =>
{
    var jobKey = new JobKey("MonthlyAttendanceJob");

    q.AddJob<MonthlyAttendanceJob>(opts =>
        opts.WithIdentity(jobKey));

    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("MonthlyAttendanceTrigger")
        .WithCronSchedule("0 0 2 21 * ?"));
});

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

// Static Files
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(@"C:\HRMSFiles"),
    RequestPath = "/HRMSFiles"
});

app.UseRotativa();

// ===============================
// 🔥 HANGFIRE DASHBOARD
// ===============================
app.UseHangfireDashboard("/hangfire");

// ===============================
// 🔥 HANGFIRE RECURRING JOB
// ===============================
RecurringJob.AddOrUpdate<RelievingLetterHangfireJob>(
    "relieving-letter-job",
    job => job.ExecuteAsync(),
    "0 9 * * *" // Every day 9 AM
   // "*/1 * * * *"//Every 1 min
);

// ROUTING
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();


