using MediatR;
using Microsoft.EntityFrameworkCore;
using InterRapidisimo.Application.Common.Interfaces;
using InterRapidisimo.Domain.Exceptions;

namespace InterRapidisimo.Application.Students.Commands;

public record DeleteStudentCommand(int Id) : IRequest<bool>;

public class DeleteStudentCommandHandler : IRequestHandler<DeleteStudentCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteStudentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteStudentCommand request, CancellationToken cancellationToken)
    {
        var student = await _context.Students
            .Include(s => s.Enrollments)
                .ThenInclude(e => e.EnrollmentDetails)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (student == null)
        {
            throw new BusinessRuleException($"No se encontró el estudiante con ID {request.Id}.");
        }

        _context.Students.Remove(student);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
