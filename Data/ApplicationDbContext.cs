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
        // Enable Retry on Failure
        // =======================
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(
                    "",
                    sql => sql.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null
                    )
                );
            }
        }

        // DbSets
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Payroll> Payrolls { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Leave> Leaves { get; set; }
        public DbSet<GurukulVideo> GurukulVideos { get; set; }
        public DbSet<GurukulProgress> GurukulProgress {  get; set; }
        public DbSet<Expenses> Expenses { get; set; }

        public DbSet<GurukulVideo> GurukulVideos { get; set; }

        public DbSet<GurukulProgress> GurukulProgress { get; set; }
        public DbSet<LeaveApprovalRoute> LeaveApprovalRoutes { get; set; }
        public DbSet<Hr> Hrs { get; set; }
        public DbSet<GeoTag> GeoTags => Set<GeoTag>();
        public DbSet<Leave> LeaveResults { get; set; }
       
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Relations
            // =============================================
            // 1️⃣ Configure Attendance as KEYLESS ENTITY
            // =============================================
            modelBuilder.Entity<Attendance>()
                .HasNoKey()
                .ToTable("Attendances"); // 🔥 ensures proper table mapping

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

           

            // Explicit table mapping
            modelBuilder.Entity<Leave>().ToTable("Leaves");
        }
    }
}

//using HRMS.Models;
//using Microsoft.EntityFrameworkCore;

//namespace HRMS.Data
//{
//    public class ApplicationDbContext : DbContext
//    {
//        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
//            : base(options)
//        {
//        }

//        public DbSet<Attendance> Attendances { get; set; }
//        public DbSet<Employee> Employees { get; set; }
//        public DbSet<Leave> Leaves { get; set; }
//        public DbSet<Expenses> Expenses { get; set; }
//        public DbSet<Payroll> Payrolls { get; set; }
//        public DbSet<LeaveApprovalRoute> LeaveApprovalRoutes { get; set; }
//        public DbSet<Hr> Hrs { get; set; }
//        public DbSet<GeoTag> GeoTags => Set<GeoTag>();

//        protected override void OnModelCreating(ModelBuilder modelBuilder)
//        {
//            base.OnModelCreating(modelBuilder);

//            // ======================================================
//            // ATTENDANCE TABLE MAPPING (MATCHES SQL EXACTLY)
//            // ======================================================

//            modelBuilder.Entity<Attendance>(entity =>
//            {
//                // Your SQL table ALREADY has PK (Emp_Code + Date)
//                entity.HasKey(a => new { a.EmpCode, a.Date });

//                // Prevent EF from altering this table in migrations
//                entity.ToTable("Attendances", t => t.ExcludeFromMigrations());

//                // Column mappings
//                entity.Property(a => a.EmpCode).HasColumnName("Emp_Code");
//                entity.Property(a => a.Date).HasColumnName("Date");
//                entity.Property(a => a.Status).HasColumnName("Status");
//                entity.Property(a => a.InTime).HasColumnName("InTime");
//                entity.Property(a => a.OutTime).HasColumnName("OutTime");
//                entity.Property(a => a.TotalHours).HasColumnName("Total_Hours");
//            });

//            // ======================================================
//            // LEAVE FOREIGN KEY MAPPING
//            // ======================================================

//            modelBuilder.Entity<Leave>()
//                .HasOne(l => l.Employee)
//                .WithMany()
//                .HasForeignKey(l => l.EmployeeId)
//                .OnDelete(DeleteBehavior.Restrict);
//        }
//    }
//}
