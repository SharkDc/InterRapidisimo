using MediatR;
using Microsoft.EntityFrameworkCore;
using InterRapidisimo.Application.Common.Interfaces;
using InterRapidisimo.Application.Courses.DTOs;
using InterRapidisimo.Application.Students.DTOs;

namespace InterRapidisimo.Application.Students.Queries;

public record GetAllStudentsQuery : IRequest<List<StudentDto>>;

public class GetAllStudentsQueryHandler : IRequestHandler<GetAllStudentsQuery, List<StudentDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllStudentsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<StudentDto>> Handle(GetAllStudentsQuery request, CancellationToken cancellationToken)
    {
        var students = await _context.Students
            .AsNoTracking()
            .Include(s => s.Enrollments)
                .ThenInclude(e => e.EnrollmentDetails)
                    .ThenInclude(ed => ed.Course)
                        .ThenInclude(c => c.Teacher)
            .OrderByDescending(s => s.RegistrationDate)
            .ToListAsync(cancellationToken);

        return students.Select(s =>
        {
            var latestEnrollment = s.Enrollments.OrderByDescending(e => e.EnrollmentDate).FirstOrDefault();
            var courses = latestEnrollment?.EnrollmentDetails
                .Select(ed => new CourseDto
                {
                    Id = ed.Course.Id,
                    Name = ed.Course.Name,
                    Credits = ed.Course.Credits,
                    TeacherId = ed.Course.TeacherId,
                    TeacherName = ed.Course.Teacher.FullName
                }).ToList() ?? new List<CourseDto>();

            return new StudentDto
            {
                Id = s.Id,
                DocumentNumber = s.DocumentNumber,
                FullName = s.FullName,
                Email = s.Email,
                Phone = s.Phone,
                RegistrationDate = s.RegistrationDate,
                TotalCredits = latestEnrollment?.TotalCredits ?? 0,
                Courses = courses
            };
        }).ToList();
    }
}
