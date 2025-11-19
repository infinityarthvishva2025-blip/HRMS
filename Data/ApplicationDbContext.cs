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
        public DbSet<Payroll> Payrolls { get; set; }
        public DbSet<HRMS.Models.Employee> Employees { get; set; }
        //public DbSet<HRMS.Models.Leave> Leaves { get; set; }
      
        public DbSet<Hr> Hrs { get; set; }
        public DbSet<GeoTag> GeoTags => Set<GeoTag>();

      

        public DbSet<Leave> Leaves { get; set; } = null!;
        public DbSet<Expenses> Expenses { get; set; }
        public DbSet<LeaveApprovalRoute> LeaveApprovalRoutes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Leave>()
                .HasOne(l => l.Employee)
                .WithMany()
                .HasForeignKey(l => l.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        public DbSet<GurukulVideo> GurukulVideos { get; set; }
        public DbSet<GurukulProgress> GurukulProgress { get; set; }


    }
}
