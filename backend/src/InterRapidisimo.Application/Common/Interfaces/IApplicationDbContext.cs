using Microsoft.EntityFrameworkCore;
using InterRapidisimo.Domain.Entities;

namespace InterRapidisimo.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Teacher> Teachers { get; }
    DbSet<Course> Courses { get; }
    DbSet<Student> Students { get; }
    DbSet<Enrollment> Enrollments { get; }
    DbSet<EnrollmentDetail> EnrollmentDetails { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
