using Microsoft.EntityFrameworkCore;
using InterRapidisimo.Application.Common.Interfaces;
using InterRapidisimo.Domain.Entities;

namespace InterRapidisimo.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Teacher> Teachers => Set<Teacher>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<EnrollmentDetail> EnrollmentDetails => Set<EnrollmentDetail>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Teacher
        modelBuilder.Entity<Teacher>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.FullName).IsRequired().HasMaxLength(150);
            entity.Property(t => t.Email).IsRequired().HasMaxLength(150);
            entity.HasIndex(t => t.Email).IsUnique();
        });

        // Course
        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(150);
            entity.Property(c => c.Credits).IsRequired().HasDefaultValue(3);

            entity.HasOne(c => c.Teacher)
                  .WithMany(t => t.Courses)
                  .HasForeignKey(c => c.TeacherId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Student
        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.DocumentNumber).IsRequired().HasMaxLength(20);
            entity.Property(s => s.FullName).IsRequired().HasMaxLength(150);
            entity.Property(s => s.Email).IsRequired().HasMaxLength(150);
            entity.Property(s => s.Phone).HasMaxLength(20);

            entity.HasIndex(s => s.DocumentNumber).IsUnique();
            entity.HasIndex(s => s.Email).IsUnique();
        });

        // Enrollment
        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TotalCredits).IsRequired().HasDefaultValue(9);

            entity.HasOne(e => e.Student)
                  .WithMany(s => s.Enrollments)
                  .HasForeignKey(e => e.StudentId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // EnrollmentDetail
        modelBuilder.Entity<EnrollmentDetail>(entity =>
        {
            entity.HasKey(ed => ed.Id);

            entity.HasOne(ed => ed.Enrollment)
                  .WithMany(e => e.EnrollmentDetails)
                  .HasForeignKey(ed => ed.EnrollmentId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ed => ed.Course)
                  .WithMany(c => c.EnrollmentDetails)
                  .HasForeignKey(ed => ed.CourseId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
