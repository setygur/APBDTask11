using Microsoft.EntityFrameworkCore;
using APBDTask11.Database.Models;

namespace APBDTask11.Database
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Device> Devices { get; set; }
        public DbSet<DeviceType> DeviceTypes { get; set; }
        public DbSet<DeviceEmployee> DeviceEmployees { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Position> Positions { get; set; }
        public DbSet<Person> People { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DeviceEmployee>()
                .HasOne(de => de.Device)
                .WithMany(d => d.DeviceEmployees)
                .HasForeignKey(de => de.DeviceId);

            modelBuilder.Entity<DeviceEmployee>()
                .HasOne(de => de.Employee)
                .WithMany(e => e.DeviceEmployees)
                .HasForeignKey(de => de.EmployeeId);

            modelBuilder.Entity<Account>()
                .HasOne(a => a.Employee)
                .WithMany(e => e.Accounts)
                .HasForeignKey(a => a.EmployeeId);

            modelBuilder.Entity<Account>()
                .HasOne(a => a.Role)
                .WithMany(r => r.Accounts)
                .HasForeignKey(a => a.RoleId);

            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Position)
                .WithMany(p => p.Employees)
                .HasForeignKey(e => e.PositionId);

            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Person)
                .WithMany(p => p.Employees)
                .HasForeignKey(e => e.PersonId);
        }
    }
}