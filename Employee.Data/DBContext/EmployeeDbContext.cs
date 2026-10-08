using Microsoft.EntityFrameworkCore;
using Employee.Data.Models;

namespace Employee.Data.Data
{
    public class EmployeeDbContext : DbContext
    {
        public EmployeeDbContext(
            DbContextOptions<EmployeeDbContext> options)
            : base(options)
        {
        }

        public DbSet<Employee1> Employees { get; set; }

        public DbSet<User> Users { get; set; }


        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<User>()
                .ToTable("TblUsers");

            modelBuilder.Entity<User>()
                .HasKey(x => x.Id);



            modelBuilder.Entity<Employee1>()
                .ToTable("Employees");

            modelBuilder.Entity<Employee1>()
                .HasKey(x => x.Id);


            modelBuilder.Entity<User>()
                .HasIndex(x => x.Email)
                .IsUnique();



            modelBuilder.Entity<Employee1>()
                .HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);




            modelBuilder.Entity<Employee1>()
                .HasOne(x => x.Manager)
                .WithMany(x => x.Employees)
                .HasForeignKey(x => x.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);




            modelBuilder.Entity<User>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.AdminId)
                .OnDelete(DeleteBehavior.Restrict);



            modelBuilder.Entity<User>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);



            modelBuilder.Entity<User>()
                .Property(x => x.Role)
                .HasDefaultValue("Employee");
        }
    }
}







