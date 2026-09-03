# Reglas de Desarrollo Backend - Inter Rapidísimo (.NET 10 / CQRS)

Estas reglas se aplican a todo desarrollo o modificación en la carpeta `backend/`.

## Directrices del Agente Backend

1. **Clean Architecture Estricta**:
   - `InterRapidisimo.Domain`: Entidades (`Student`, `Teacher`, `Course`, `Enrollment`, `EnrollmentDetail`) y excepciones de dominio. Ninguna dependencia externa.
   - `InterRapidisimo.Application`: Lógica de aplicación mediante CQRS con **MediatR**. Validaciones automáticas con **FluentValidation** en el pipeline (`ValidationBehavior`).
   - `InterRapidisimo.Infrastructure`: `ApplicationDbContext`, configuración de Fluent API, seeder `DbInitializer` y selector SQLite / SQL Server.
   - `InterRapidisimo.Api`: Controladores delgados que solo despachan al Mediator. Middleware global de excepciones.

2. **Validación Innegociable de Reglas de Negocio**:
   - Todo estudiante debe tener exactamente 3 materias (9 créditos totales).
   - Prohibido matricular materias con el mismo docente (lanzar `BusinessRuleException`).
   - La consulta de compañeros de clase (`GetStudentClassmatesQuery`) solo expone nombres.

3. **Pruebas Automatizadas Obligatorias**:
   - Cada nueva regla o modificación debe contar con su correspondiente prueba unitaria en `InterRapidisimo.Tests`.
   - Ejecutar siempre:
     ```powershell
     dotnet test
     ```
