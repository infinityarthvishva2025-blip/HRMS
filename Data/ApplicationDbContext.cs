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

        // =======================
        // TABLE MAPPINGS
        // =======================
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Payroll> Payrolls { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Hr> Hrs { get; set; }
        public DbSet<GeoTag> GeoTags => Set<GeoTag>();
        public DbSet<Leave> Leaves { get; set; }
        public DbSet<Expenses> Expenses { get; set; }
        public DbSet<LeaveApprovalRoute> LeaveApprovalRoutes { get; set; }
        public DbSet<GurukulVideo> GurukulVideos { get; set; }
        public DbSet<GurukulProgress> GurukulProgress { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =============================================
            // 1️⃣ Configure Attendance as KEYLESS ENTITY
            // =============================================
            modelBuilder.Entity<Attendance>()
                .HasNoKey()
                .ToTable("Attendances"); // 🔥 ensures proper table mapping

            // =============================================
            // 2️⃣ Map column names EXACTLY as SQL table
            // =============================================
            modelBuilder.Entity<Attendance>().Property(a => a.Emp_Code).HasColumnName("Emp_Code");
            modelBuilder.Entity<Attendance>().Property(a => a.Date).HasColumnName("Date");
            modelBuilder.Entity<Attendance>().Property(a => a.Status).HasColumnName("Status");
            modelBuilder.Entity<Attendance>().Property(a => a.InTime).HasColumnName("InTime");
            modelBuilder.Entity<Attendance>().Property(a => a.OutTime).HasColumnName("OutTime");
            modelBuilder.Entity<Attendance>().Property(a => a.Total_Hours).HasColumnName("Total_Hours");

            // =============================================
            // 3️⃣ Configure Leave foreign key properly
            // =============================================
            modelBuilder.Entity<Leave>()
                .HasOne(l => l.Employee)
                .WithMany()
                .HasForeignKey(l => l.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
