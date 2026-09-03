using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using InterRapidisimo.Application.Students.Commands;
using InterRapidisimo.Application.Students.Queries;
using InterRapidisimo.Domain.Entities;
using InterRapidisimo.Domain.Exceptions;
using InterRapidisimo.Infrastructure.Persistence;

namespace InterRapidisimo.Tests;

public class StudentEnrollmentTests
{
    private ApplicationDbContext GetInMemoryDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        var context = new ApplicationDbContext(options);
        SeedSampleData(context);
        return context;
    }

    private void SeedSampleData(ApplicationDbContext context)
    {
        if (context.Teachers.Any()) return;

        // 5 profesores
        var t1 = new Teacher { Id = 1, FullName = "Dr. Carlos Mendoza", Email = "carlos@mail.com" };
        var t2 = new Teacher { Id = 2, FullName = "Ing. Laura Gómez", Email = "laura@mail.com" };
        var t3 = new Teacher { Id = 3, FullName = "Dr. Roberto Silva", Email = "roberto@mail.com" };
        var t4 = new Teacher { Id = 4, FullName = "Dra. Diana Torres", Email = "diana@mail.com" };
        var t5 = new Teacher { Id = 5, FullName = "Mg. Andrés Morales", Email = "andres@mail.com" };
        context.Teachers.AddRange(t1, t2, t3, t4, t5);

        // 10 materias (2 por profesor, 3 créditos cada una)
        var c1 = new Course { Id = 1, Name = "Cálculo Diferencial", Credits = 3, TeacherId = 1, Teacher = t1 };
        var c2 = new Course { Id = 2, Name = "Álgebra Lineal", Credits = 3, TeacherId = 1, Teacher = t1 };
        var c3 = new Course { Id = 3, Name = "Algoritmos", Credits = 3, TeacherId = 2, Teacher = t2 };
        var c4 = new Course { Id = 4, Name = "POO", Credits = 3, TeacherId = 2, Teacher = t2 };
        var c5 = new Course { Id = 5, Name = "Física Mecánica", Credits = 3, TeacherId = 3, Teacher = t3 };
        var c6 = new Course { Id = 6, Name = "Electromagnetismo", Credits = 3, TeacherId = 3, Teacher = t3 };
        var c7 = new Course { Id = 7, Name = "Bases de Datos", Credits = 3, TeacherId = 4, Teacher = t4 };
        var c8 = new Course { Id = 8, Name = "Arquitectura de Software", Credits = 3, TeacherId = 4, Teacher = t4 };
        var c9 = new Course { Id = 9, Name = "Redes", Credits = 3, TeacherId = 5, Teacher = t5 };
        var c10 = new Course { Id = 10, Name = "Requisitos", Credits = 3, TeacherId = 5, Teacher = t5 };
        context.Courses.AddRange(c1, c2, c3, c4, c5, c6, c7, c8, c9, c10);

        context.SaveChanges();
    }

    [Fact]
    public void Validator_Should_Fail_When_Courses_Count_Is_Not_3()
    {
        // Arrange
        var validator = new CreateStudentCommandValidator();
        var commandLessThan3 = new CreateStudentCommand
        {
            DocumentNumber = "12345",
            FullName = "Test Student",
            Email = "test@student.com",
            CourseIds = new List<int> { 1, 3 } // Solo 2 materias
        };

        var commandMoreThan3 = new CreateStudentCommand
        {
            DocumentNumber = "12345",
            FullName = "Test Student",
            Email = "test@student.com",
            CourseIds = new List<int> { 1, 3, 5, 7 } // 4 materias
        };

        // Act
        var resultLess = validator.Validate(commandLessThan3);
        var resultMore = validator.Validate(commandMoreThan3);

        // Assert
        resultLess.IsValid.Should().BeFalse();
        resultLess.Errors.Should().Contain(e => e.ErrorMessage.Contains("exactamente 3 materias"));

        resultMore.IsValid.Should().BeFalse();
        resultMore.Errors.Should().Contain(e => e.ErrorMessage.Contains("exactamente 3 materias"));
    }

    [Fact]
    public void Validator_Should_Fail_When_Courses_Contain_Duplicates()
    {
        // Arrange
        var validator = new CreateStudentCommandValidator();
        var command = new CreateStudentCommand
        {
            DocumentNumber = "12345",
            FullName = "Test Student",
            Email = "test@student.com",
            CourseIds = new List<int> { 1, 1, 3 } // Materia 1 repetida
        };

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("duplicadas"));
    }

    [Fact]
    public async Task Handler_Should_Fail_When_Courses_Share_Same_Teacher()
    {
        // Arrange
        var context = GetInMemoryDbContext("TestDb_SameTeacher");
        var handler = new CreateStudentCommandHandler(context);

        // Cursos 1 (Cálculo) y 2 (Álgebra) son ambos del Profesor 1 (Dr. Carlos Mendoza)
        var command = new CreateStudentCommand
        {
            DocumentNumber = "987654321",
            FullName = "Estudiante Conflicto",
            Email = "conflicto@test.com",
            Phone = "3001234567",
            CourseIds = new List<int> { 1, 2, 3 } // 1 y 2 tienen el mismo profesor!
        };

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert: Regla 7 (El estudiante no podrá tener clases con el mismo profesor)
        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*no podrá tener clases con el mismo profesor*Dr. Carlos Mendoza*");
    }

    [Fact]
    public async Task Handler_Should_Succeed_When_3_Courses_Have_Distinct_Teachers()
    {
        // Arrange
        var context = GetInMemoryDbContext("TestDb_Success");
        var handler = new CreateStudentCommandHandler(context);

        // Curso 1 (Prof 1), Curso 3 (Prof 2), Curso 5 (Prof 3)
        var command = new CreateStudentCommand
        {
            DocumentNumber = "1098765432",
            FullName = "Pedro Pérez",
            Email = "pedro.perez@test.com",
            Phone = "3112233445",
            CourseIds = new List<int> { 1, 3, 5 } // Profesores 1, 2 y 3
        };

        // Act
        var studentId = await handler.Handle(command, CancellationToken.None);

        // Assert
        studentId.Should().BeGreaterThan(0);

        var savedStudent = await context.Students
            .Include(s => s.Enrollments)
                .ThenInclude(e => e.EnrollmentDetails)
            .FirstOrDefaultAsync(s => s.Id == studentId);

        savedStudent.Should().NotBeNull();
        savedStudent!.FullName.Should().Be("Pedro Pérez");
        savedStudent.Enrollments.Should().HaveCount(1);

        var enrollment = savedStudent.Enrollments.First();
        enrollment.TotalCredits.Should().Be(9); // 3 materias * 3 créditos = 9
        enrollment.EnrollmentDetails.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handler_Should_Fail_When_Document_Already_Exists()
    {
        // Arrange
        var context = GetInMemoryDbContext("TestDb_DuplicateDoc");
        var handler = new CreateStudentCommandHandler(context);

        var existingStudent = new Student
        {
            DocumentNumber = "555666777",
            FullName = "Estudiante Existente",
            Email = "existente@test.com"
        };
        context.Students.Add(existingStudent);
        await context.SaveChangesAsync();

        var command = new CreateStudentCommand
        {
            DocumentNumber = "555666777", // Mismo documento
            FullName = "Otro Estudiante",
            Email = "otro@test.com",
            CourseIds = new List<int> { 1, 3, 5 }
        };

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*Ya existe un estudiante registrado con el documento*");
    }

    [Fact]
    public async Task Classmates_Query_Should_Return_Only_Classmate_Names_Excluding_Self()
    {
        // Arrange
        var context = GetInMemoryDbContext("TestDb_Classmates");

        // Crear dos estudiantes compartiendo Curso 1 (Cálculo)
        var studentA = new Student { Id = 10, DocumentNumber = "111", FullName = "Ana Martínez", Email = "ana@test.com" };
        var studentB = new Student { Id = 20, DocumentNumber = "222", FullName = "Bernardo Silva", Email = "bernardo@test.com" };
        var studentC = new Student { Id = 30, DocumentNumber = "333", FullName = "Carlos Ortega", Email = "carlos_o@test.com" };
        context.Students.AddRange(studentA, studentB, studentC);
        await context.SaveChangesAsync();

        // Matrícula de Ana: Curso 1, Curso 3, Curso 5
        context.Enrollments.Add(new Enrollment
        {
            StudentId = 10,
            EnrollmentDetails = new List<EnrollmentDetail>
            {
                new EnrollmentDetail { CourseId = 1 },
                new EnrollmentDetail { CourseId = 3 },
                new EnrollmentDetail { CourseId = 5 }
            }
        });

        // Matrícula de Bernardo: Curso 1, Curso 4, Curso 7 (comparte Curso 1 con Ana)
        context.Enrollments.Add(new Enrollment
        {
            StudentId = 20,
            EnrollmentDetails = new List<EnrollmentDetail>
            {
                new EnrollmentDetail { CourseId = 1 },
                new EnrollmentDetail { CourseId = 4 },
                new EnrollmentDetail { CourseId = 7 }
            }
        });

        // Matrícula de Carlos: Curso 1, Curso 3, Curso 8 (comparte Curso 1 y 3 con Ana)
        context.Enrollments.Add(new Enrollment
        {
            StudentId = 30,
            EnrollmentDetails = new List<EnrollmentDetail>
            {
                new EnrollmentDetail { CourseId = 1 },
                new EnrollmentDetail { CourseId = 3 },
                new EnrollmentDetail { CourseId = 8 }
            }
        });

        await context.SaveChangesAsync();

        var queryHandler = new GetStudentClassmatesQueryHandler(context);

        // Act: Consultar compañeros para Ana (ID: 10)
        var result = await queryHandler.Handle(new GetStudentClassmatesQuery(10), CancellationToken.None);

        // Assert: Requerimiento 9
        result.Should().NotBeNull();
        result.StudentName.Should().Be("Ana Martínez");
        result.Courses.Should().HaveCount(3);

        // En Curso 1 (Cálculo), los compañeros son Bernardo y Carlos (NO Ana)
        var curso1 = result.Courses.First(c => c.CourseId == 1);
        curso1.Classmates.Should().BeEquivalentTo(new[] { "Bernardo Silva", "Carlos Ortega" });
        curso1.Classmates.Should().NotContain("Ana Martínez");

        // En Curso 3 (Algoritmos), el compañero es Carlos (Bernardo no está en Curso 3)
        var curso3 = result.Courses.First(c => c.CourseId == 3);
        curso3.Classmates.Should().BeEquivalentTo(new[] { "Carlos Ortega" });

        // En Curso 5 (Física), nadie más está matriculado
        var curso5 = result.Courses.First(c => c.CourseId == 5);
        curso5.Classmates.Should().BeEmpty();
    }
}
