---
name: backend-engineer
description: "Desarrollo y mantenimiento de funcionalidades backend en .NET 10 con CQRS, MediatR, FluentValidation, Entity Framework Core y pruebas automatizadas con xUnit para Inter Rapidísimo."
---

# Habilidad: Ingeniero Backend (Inter Rapidísimo)

Usa esta habilidad para implementar nuevos comandos, consultas, validadores, configuraciones de base de datos o pruebas unitarias en el backend.

## Flujo para Crear un Nuevo Comando CQRS

### 1. Definir el Record del Comando
En `backend/src/InterRapidisimo.Application/{Feature}/Commands/{Nombre}Command.cs`:
```csharp
public record MiAccionCommand(string Param1, List<int> CourseIds) : IRequest<MiResultadoDto>;
```

### 2. Implementar el Validador FluentValidation
```csharp
public class MiAccionCommandValidator : AbstractValidator<MiAccionCommand>
{
    public MiAccionCommandValidator()
    {
        RuleFor(x => x.Param1).NotEmpty().WithMessage("El parámetro es obligatorio.");
        RuleFor(x => x.CourseIds)
            .NotNull()
            .Must(ids => ids.Count == 3).WithMessage("Debe registrar exactamente 3 materias.")
            .Must(ids => ids.Distinct().Count() == 3).WithMessage("No se permiten materias duplicadas.");
    }
}
```

### 3. Implementar el Handler
```csharp
public class MiAccionCommandHandler : IRequestHandler<MiAccionCommand, MiResultadoDto>
{
    private readonly ApplicationDbContext _context;

    public MiAccionCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MiResultadoDto> Handle(MiAccionCommand request, CancellationToken cancellationToken)
    {
        // 1. Validar regla de profesores distintos
        var courses = await _context.Courses.Where(c => request.CourseIds.Contains(c.Id)).ToListAsync(cancellationToken);
        if (courses.Select(c => c.TeacherId).Distinct().Count() < courses.Count)
        {
            throw new BusinessRuleException("No se permite ver materias con el mismo docente.");
        }

        // 2. Persistir entidad
        // 3. Retornar DTO
    }
}
```

## Flujo para Crear una Prueba Unitaria
En `backend/tests/InterRapidisimo.Tests/`:
```csharp
[Fact]
public async Task MiAccion_ConProfesoresRepetidos_DebeLanzarBusinessRuleException()
{
    // Arrange
    // Act & Assert
    await Assert.ThrowsAsync<BusinessRuleException>(() => handler.Handle(command, CancellationToken.None));
}
```

## Comandos de Verificación
```powershell
cd backend
dotnet test
```
