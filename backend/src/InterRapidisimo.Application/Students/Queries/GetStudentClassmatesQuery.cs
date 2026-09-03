using MediatR;
using Microsoft.EntityFrameworkCore;
using InterRapidisimo.Application.Common.Interfaces;
using InterRapidisimo.Application.Students.DTOs;
using InterRapidisimo.Domain.Exceptions;

namespace InterRapidisimo.Application.Students.Queries;

public record GetStudentClassmatesQuery(int StudentId) : IRequest<StudentClassmatesDto>;

public class GetStudentClassmatesQueryHandler : IRequestHandler<GetStudentClassmatesQuery, StudentClassmatesDto>
{
    private readonly IApplicationDbContext _context;

    public GetStudentClassmatesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StudentClassmatesDto> Handle(GetStudentClassmatesQuery request, CancellationToken cancellationToken)
    {
        var student = await _context.Students
            .AsNoTracking()
            .Include(s => s.Enrollments)
                .ThenInclude(e => e.EnrollmentDetails)
                    .ThenInclude(ed => ed.Course)
                        .ThenInclude(c => c.Teacher)
            .FirstOrDefaultAsync(s => s.Id == request.StudentId, cancellationToken);

        if (student == null)
        {
            throw new BusinessRuleException($"No se encontró el estudiante con ID {request.StudentId}.");
        }

        var latestEnrollment = student.Enrollments.OrderByDescending(e => e.EnrollmentDate).FirstOrDefault();
        var enrolledCourseIds = latestEnrollment?.EnrollmentDetails.Select(ed => ed.CourseId).ToList() 
            ?? new List<int>();

        // Para cada materia matriculada por el estudiante, consultar ÚNICAMENTE los nombres de los compañeros (Requerimiento 9)
        var coursesClassmates = new List<CourseClassmatesDto>();

        foreach (var courseId in enrolledCourseIds)
        {
            var course = await _context.Courses
                .Include(c => c.Teacher)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == courseId, cancellationToken);

            if (course == null) continue;

            // Consultar únicamente los nombres de los alumnos con quienes compartirá la clase (excluyendo al estudiante actual)
            var classmates = await _context.EnrollmentDetails
                .AsNoTracking()
                .Where(ed => ed.CourseId == courseId && ed.Enrollment.StudentId != request.StudentId)
                .Select(ed => ed.Enrollment.Student.FullName)
                .Distinct()
                .OrderBy(name => name)
                .ToListAsync(cancellationToken);

            coursesClassmates.Add(new CourseClassmatesDto
            {
                CourseId = course.Id,
                CourseName = course.Name,
                TeacherName = course.Teacher.FullName,
                Classmates = classmates
            });
        }

        return new StudentClassmatesDto
        {
            StudentId = student.Id,
            StudentName = student.FullName,
            Courses = coursesClassmates
        };
    }
}
