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
       
        public DbSet<Expenses> Expenses { get; set; }

        public DbSet<Assets> Assets { get; set; }
   
        public DbSet<Payroll> Payrolls { get; set; }
        public DbSet<HRMS.Models.Employee> Employees { get; set; }
        public DbSet<HRMS.Models.Leave> Leaves { get; set; }
        public DbSet<Hr> Hrs { get; set; }
        public DbSet<GeoTag> GeoTags => Set<GeoTag>();
      


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<GeoTag>()
                .HasIndex(g => g.TagId)
                .IsUnique();

            // sample seed (optional)
            modelBuilder.Entity<GeoTag>().HasData(
                new GeoTag { Id = 1, TagId = "Office-001", Latitude = 19.0760, Longitude = 72.8777, RadiusMeters = 100000, Description = "Mumbai Office" }
            );
        }

    }
}
