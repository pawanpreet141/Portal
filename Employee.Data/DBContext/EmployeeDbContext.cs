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

        public DbSet<PasswordResetOtp> PasswordResetOtps { get; set; }

        public DbSet<LeaveRequest> LeaveRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // USER TABLE
            modelBuilder.Entity<User>().ToTable("TblUsers");

            modelBuilder.Entity<User>()
                .HasKey(x => x.Id);

            modelBuilder.Entity<User>()
                .Property(x => x.Name)
                .HasMaxLength(100);

            modelBuilder.Entity<User>()
                .Property(x => x.Email)
                .HasMaxLength(150)
                .IsRequired();

            modelBuilder.Entity<User>()
                .Property(x => x.PasswordHash)
                .HasMaxLength(255)
                .IsRequired();

            modelBuilder.Entity<User>()
                .Property(x => x.Role)
                .HasMaxLength(50)
                .HasDefaultValue("Employee");

            modelBuilder.Entity<User>()
                .Property(x => x.Department)
                .HasMaxLength(100)
                .IsRequired(false);

            modelBuilder.Entity<User>()
                .Property(x => x.Team)
                .HasMaxLength(100)
                .IsRequired(false);

            modelBuilder.Entity<User>()
                .HasIndex(x => x.Email)
                .IsUnique();

            // EMPLOYEE TABLE
            modelBuilder.Entity<Employee1>().ToTable("Employees");

            modelBuilder.Entity<Employee1>()
                .HasKey(x => x.Id);

            modelBuilder.Entity<Employee1>()
                .Property(x => x.Name)
                .HasMaxLength(100);

            modelBuilder.Entity<Employee1>()
                .Property(x => x.Email)
                .HasMaxLength(100);

            modelBuilder.Entity<Employee1>()
                .Property(x => x.Department)
                .HasMaxLength(100);

            modelBuilder.Entity<Employee1>()
                .Property(x => x.Team)
                .HasMaxLength(100)
                .IsRequired(false);

            modelBuilder.Entity<Employee1>()
                .Property(x => x.Salary)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Employee1>()
                .Property(x => x.CreatedAt)
                .HasColumnType("datetime");

            modelBuilder.Entity<Employee1>()
                .Property(x => x.UpdatedAt)
                .HasColumnType("datetime");

            modelBuilder.Entity<Employee1>()
                .Property(x => x.CreatedBy);    

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

            modelBuilder.Entity<Employee1>()
                 .HasOne<User>()
                 .WithMany()
                 .HasForeignKey(x => x.LoginUserId)
                 .OnDelete(DeleteBehavior.Restrict);

            // USER SELF-RELATIONSHIPS
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

            // PASSWORD RESET OTP TABLE
            modelBuilder.Entity<PasswordResetOtp>()
                .ToTable("PasswordResetOtps");

            modelBuilder.Entity<PasswordResetOtp>()
                .HasKey(x => x.Id);

            modelBuilder.Entity<PasswordResetOtp>()
                .HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PasswordResetOtp>()
                .HasIndex(x => x.UserId);

            modelBuilder.Entity<PasswordResetOtp>()
                .Property(x => x.OtpHash)
                .IsRequired()
                .HasMaxLength(64);

            // LEAVE REQUEST TABLE
            modelBuilder.Entity<LeaveRequest>()
                .ToTable("TblLeaveRequests");

            modelBuilder.Entity<LeaveRequest>()
                .HasKey(x => x.Id);

            modelBuilder.Entity<LeaveRequest>()
                .HasOne(x => x.Employee)
                .WithMany()
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}







