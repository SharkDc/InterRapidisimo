using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using InterRapidisimo.Application.Common.Interfaces;
using InterRapidisimo.Domain.Entities;
using InterRapidisimo.Domain.Exceptions;

namespace InterRapidisimo.Application.Students.Commands;

public record UpdateStudentCommand : IRequest<bool>
{
    public int Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public List<int> CourseIds { get; init; } = new();
}

public class UpdateStudentCommandValidator : AbstractValidator<UpdateStudentCommand>
{
    public UpdateStudentCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("El ID de estudiante no es válido.");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("El nombre completo es obligatorio.")
            .MinimumLength(3).WithMessage("El nombre completo debe tener al menos 3 caracteres.")
            .MaximumLength(150).WithMessage("El nombre completo no debe exceder 150 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El correo electrónico es obligatorio.")
            .EmailAddress().WithMessage("El formato del correo electrónico no es válido.")
            .MaximumLength(150).WithMessage("El correo no debe exceder 150 caracteres.");

        RuleFor(x => x.CourseIds)
            .NotNull().WithMessage("Debe seleccionar las materias a matricular.")
            .Must(c => c != null && c.Count == 3)
            .WithMessage("El estudiante sólo podrá seleccionar exactamente 3 materias (9 créditos).")
            .Must(c => c != null && c.Distinct().Count() == c.Count)
            .WithMessage("No puede seleccionar materias duplicadas.");
    }
}

public class UpdateStudentCommandHandler : IRequestHandler<UpdateStudentCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public UpdateStudentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateStudentCommand request, CancellationToken cancellationToken)
    {
        var student = await _context.Students
            .Include(s => s.Enrollments)
                .ThenInclude(e => e.EnrollmentDetails)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (student == null)
        {
            throw new BusinessRuleException($"No se encontró el estudiante con ID {request.Id}.");
        }

        // 1. Validar unicidad de correo (excluyendo al estudiante actual)
        var emailExists = await _context.Students
            .AnyAsync(s => s.Id != request.Id && s.Email.ToLower() == request.Email.Trim().ToLower(), cancellationToken);
        if (emailExists)
        {
            throw new BusinessRuleException($"El correo '{request.Email}' ya está en uso por otro estudiante.");
        }

        // 2. Consultar las materias seleccionadas
        var selectedCourses = await _context.Courses
            .Include(c => c.Teacher)
            .Where(c => request.CourseIds.Contains(c.Id))
            .ToListAsync(cancellationToken);

        if (selectedCourses.Count != 3)
        {
            throw new BusinessRuleException("Una o más materias seleccionadas no fueron encontradas en el sistema.");
        }

        // 3. Validar profesores distintos (Regla 7)
        var groupedByTeacher = selectedCourses
            .GroupBy(c => new { c.TeacherId, c.Teacher.FullName })
            .Where(g => g.Count() > 1)
            .ToList();

        if (groupedByTeacher.Any())
        {
            var conflict = groupedByTeacher.First();
            var conflictingCourses = string.Join(", ", conflict.Select(c => $"'{c.Name}'"));
            throw new BusinessRuleException(
                $"El estudiante no podrá tener clases con el mismo profesor. Se detectó conflicto con el docente '{conflict.Key.FullName}' en las materias: {conflictingCourses}.");
        }

        // 4. Actualizar datos del estudiante
        student.FullName = request.FullName.Trim();
        student.Email = request.Email.Trim().ToLower();
        student.Phone = request.Phone?.Trim() ?? string.Empty;

        // 5. Actualizar o crear la matrícula
        var enrollment = student.Enrollments.OrderByDescending(e => e.EnrollmentDate).FirstOrDefault();
        if (enrollment == null)
        {
            enrollment = new Enrollment
            {
                StudentId = student.Id,
                EnrollmentDate = DateTime.UtcNow,
                TotalCredits = selectedCourses.Sum(c => c.Credits)
            };
            student.Enrollments.Add(enrollment);
        }
        else
        {
            enrollment.TotalCredits = selectedCourses.Sum(c => c.Credits);
            _context.EnrollmentDetails.RemoveRange(enrollment.EnrollmentDetails);
        }

        enrollment.EnrollmentDetails = selectedCourses.Select(c => new EnrollmentDetail
        {
            CourseId = c.Id,
            EnrollmentId = enrollment.Id
        }).ToList();

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
