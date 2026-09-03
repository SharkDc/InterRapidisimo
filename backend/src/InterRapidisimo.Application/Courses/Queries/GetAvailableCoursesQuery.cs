using MediatR;
using Microsoft.EntityFrameworkCore;
using InterRapidisimo.Application.Common.Interfaces;
using InterRapidisimo.Application.Courses.DTOs;

namespace InterRapidisimo.Application.Courses.Queries;

public record GetAvailableCoursesQuery : IRequest<List<CourseDto>>;

public class GetAvailableCoursesQueryHandler : IRequestHandler<GetAvailableCoursesQuery, List<CourseDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAvailableCoursesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CourseDto>> Handle(GetAvailableCoursesQuery request, CancellationToken cancellationToken)
    {
        return await _context.Courses
            .Include(c => c.Teacher)
            .AsNoTracking()
            .OrderBy(c => c.TeacherId)
            .ThenBy(c => c.Name)
            .Select(c => new CourseDto
            {
                Id = c.Id,
                Name = c.Name,
                Credits = c.Credits,
                TeacherId = c.TeacherId,
                TeacherName = c.Teacher.FullName
            })
            .ToListAsync(cancellationToken);
    }
}
