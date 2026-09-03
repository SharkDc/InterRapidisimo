using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using InterRapidisimo.Application.Common.Interfaces;
using InterRapidisimo.Domain.Entities;
using InterRapidisimo.Domain.Exceptions;

namespace InterRapidisimo.Application.Students.Commands;

public record CreateStudentCommand : IRequest<int>
{
    public string DocumentNumber { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public List<int> CourseIds { get; init; } = new();
}

public class CreateStudentCommandValidator : AbstractValidator<CreateStudentCommand>
{
    public CreateStudentCommandValidator()
    {
        RuleFor(x => x.DocumentNumber)
            .NotEmpty().WithMessage("El documento de identificación es obligatorio.")
            .MaximumLength(20).WithMessage("El documento no debe exceder 20 caracteres.");

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

public class CreateStudentCommandHandler : IRequestHandler<CreateStudentCommand, int>
{
    private readonly IApplicationDbContext _context;

    public CreateStudentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreateStudentCommand request, CancellationToken cancellationToken)
    {
        // 1. Validar unicidad de documento
        var documentExists = await _context.Students
            .AnyAsync(s => s.DocumentNumber == request.DocumentNumber.Trim(), cancellationToken);
        if (documentExists)
        {
            throw new BusinessRuleException($"Ya existe un estudiante registrado con el documento '{request.DocumentNumber}'.");
        }

        // 2. Validar unicidad de email
        var emailExists = await _context.Students
            .AnyAsync(s => s.Email.ToLower() == request.Email.Trim().ToLower(), cancellationToken);
        if (emailExists)
        {
            throw new BusinessRuleException($"Ya existe un estudiante registrado con el correo '{request.Email}'.");
        }

        // 3. Consultar las materias seleccionadas
        var selectedCourses = await _context.Courses
            .Include(c => c.Teacher)
            .Where(c => request.CourseIds.Contains(c.Id))
            .ToListAsync(cancellationToken);

        if (selectedCourses.Count != 3)
        {
            throw new BusinessRuleException("Una o más materias seleccionadas no fueron encontradas en el sistema.");
        }

        // 4. Validar que las 3 materias correspondan a 3 profesores diferentes (Regla 7)
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

        // 5. Crear el estudiante
        var student = new Student
        {
            DocumentNumber = request.DocumentNumber.Trim(),
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim().ToLower(),
            Phone = request.Phone?.Trim() ?? string.Empty,
            RegistrationDate = DateTime.UtcNow
        };

        _context.Students.Add(student);
        await _context.SaveChangesAsync(cancellationToken);

        // 6. Crear la matrícula con los 9 créditos
        var enrollment = new Enrollment
        {
            StudentId = student.Id,
            EnrollmentDate = DateTime.UtcNow,
            TotalCredits = selectedCourses.Sum(c => c.Credits),
            EnrollmentDetails = selectedCourses.Select(c => new EnrollmentDetail
            {
                CourseId = c.Id
            }).ToList()
        };

        _context.Enrollments.Add(enrollment);
        await _context.SaveChangesAsync(cancellationToken);

        return student.Id;
    }
}
