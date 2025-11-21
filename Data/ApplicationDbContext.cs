using HRMS.Models;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Leave> Leaves { get; set; }
        public DbSet<Expenses> Expenses { get; set; }
        public DbSet<Payroll> Payrolls { get; set; }
        public DbSet<LeaveApprovalRoute> LeaveApprovalRoutes { get; set; }
        public DbSet<Hr> Hrs { get; set; }
        public DbSet<GeoTag> GeoTags => Set<GeoTag>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ======================================================
            // ATTENDANCE TABLE MAPPING (MATCHES SQL EXACTLY)
            // ======================================================

            modelBuilder.Entity<Attendance>(entity =>
            {
                // Your SQL table ALREADY has PK (Emp_Code + Date)
                entity.HasKey(a => new { a.EmpCode, a.Date });

                // Prevent EF from altering this table in migrations
                entity.ToTable("Attendances", t => t.ExcludeFromMigrations());

                // Column mappings
                entity.Property(a => a.EmpCode).HasColumnName("Emp_Code");
                entity.Property(a => a.Date).HasColumnName("Date");
                entity.Property(a => a.Status).HasColumnName("Status");
                entity.Property(a => a.InTime).HasColumnName("InTime");
                entity.Property(a => a.OutTime).HasColumnName("OutTime");
                entity.Property(a => a.TotalHours).HasColumnName("Total_Hours");
            });

            // ======================================================
            // LEAVE FOREIGN KEY MAPPING
            // ======================================================

            modelBuilder.Entity<Leave>()
                .HasOne(l => l.Employee)
                .WithMany()
                .HasForeignKey(l => l.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
