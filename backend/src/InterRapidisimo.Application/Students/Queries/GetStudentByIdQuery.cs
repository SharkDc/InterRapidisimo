using MediatR;
using Microsoft.EntityFrameworkCore;
using InterRapidisimo.Application.Common.Interfaces;
using InterRapidisimo.Application.Courses.DTOs;
using InterRapidisimo.Application.Students.DTOs;
using InterRapidisimo.Domain.Exceptions;

namespace InterRapidisimo.Application.Students.Queries;

public record GetStudentByIdQuery(int Id) : IRequest<StudentDto>;

public class GetStudentByIdQueryHandler : IRequestHandler<GetStudentByIdQuery, StudentDto>
{
    private readonly IApplicationDbContext _context;

    public GetStudentByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StudentDto> Handle(GetStudentByIdQuery request, CancellationToken cancellationToken)
    {
        var s = await _context.Students
            .AsNoTracking()
            .Include(s => s.Enrollments)
                .ThenInclude(e => e.EnrollmentDetails)
                    .ThenInclude(ed => ed.Course)
                        .ThenInclude(c => c.Teacher)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (s == null)
        {
            throw new BusinessRuleException($"No se encontró el estudiante con ID {request.Id}.");
        }

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
    }
}
