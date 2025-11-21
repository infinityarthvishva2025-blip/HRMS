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
                // Primary key (Emp_Code + Date)
                entity.HasKey(a => new { a.Emp_Code, a.Date });

                // Map to SQL table name
                entity.ToTable("Attendances");

                // Column mappings
                entity.Property(a => a.Emp_Code).HasColumnName("Emp_Code");
                entity.Property(a => a.Date).HasColumnName("Date");
                entity.Property(a => a.Status).HasColumnName("Status");
                entity.Property(a => a.InTime).HasColumnName("InTime");
                entity.Property(a => a.OutTime).HasColumnName("OutTime");
                entity.Property(a => a.Total_Hours).HasColumnName("Total_Hours");
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
