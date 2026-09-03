using Microsoft.EntityFrameworkCore;
using InterRapidisimo.Domain.Entities;
using InterRapidisimo.Infrastructure.Persistence;

namespace InterRapidisimo.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Ensure database created
        await context.Database.EnsureCreatedAsync();

        if (await context.Teachers.AnyAsync())
        {
            return; // DB already seeded
        }

        // 1. Seed 5 Teachers
        var teachers = new List<Teacher>
        {
            new Teacher { FullName = "Dr. Carlos Mendoza", Email = "carlos.mendoza@interrapidisimo.edu.co" },
            new Teacher { FullName = "Ing. Laura Gómez", Email = "laura.gomez@interrapidisimo.edu.co" },
            new Teacher { FullName = "Dr. Roberto Silva", Email = "roberto.silva@interrapidisimo.edu.co" },
            new Teacher { FullName = "Dra. Diana Torres", Email = "diana.torres@interrapidisimo.edu.co" },
            new Teacher { FullName = "Mg. Andrés Morales", Email = "andres.morales@interrapidisimo.edu.co" }
        };

        context.Teachers.AddRange(teachers);
        await context.SaveChangesAsync();

        // 2. Seed 10 Courses (2 per Teacher, 3 credits each)
        var courses = new List<Course>
        {
            // Prof Carlos Mendoza
            new Course { Name = "Cálculo Diferencial", Credits = 3, TeacherId = teachers[0].Id },
            new Course { Name = "Álgebra Lineal", Credits = 3, TeacherId = teachers[0].Id },

            // Prof Laura Gómez
            new Course { Name = "Algoritmos y Estructuras de Datos", Credits = 3, TeacherId = teachers[1].Id },
            new Course { Name = "Programación Orientada a Objetos", Credits = 3, TeacherId = teachers[1].Id },

            // Prof Roberto Silva
            new Course { Name = "Física Mecánica", Credits = 3, TeacherId = teachers[2].Id },
            new Course { Name = "Electromagnetismo", Credits = 3, TeacherId = teachers[2].Id },

            // Prof Diana Torres
            new Course { Name = "Bases de Datos Relacionales", Credits = 3, TeacherId = teachers[3].Id },
            new Course { Name = "Arquitectura de Software", Credits = 3, TeacherId = teachers[3].Id },

            // Prof Andrés Morales
            new Course { Name = "Redes de Computadores", Credits = 3, TeacherId = teachers[4].Id },
            new Course { Name = "Ingeniería de Requisitos", Credits = 3, TeacherId = teachers[4].Id }
        };

        context.Courses.AddRange(courses);
        await context.SaveChangesAsync();

        // 3. Seed Sample Students with valid enrollments (3 courses, distinct teachers)
        // Student 1: Juan Pérez (Courses: Cálculo [T1], Algoritmos [T2], Bases de Datos [T4])
        var student1 = new Student
        {
            DocumentNumber = "1018456789",
            FullName = "Juan Pablo Pérez",
            Email = "juan.perez@correo.com",
            Phone = "3101234567",
            RegistrationDate = DateTime.UtcNow.AddDays(-5)
        };
        context.Students.Add(student1);
        await context.SaveChangesAsync();

        var enrollment1 = new Enrollment
        {
            StudentId = student1.Id,
            EnrollmentDate = DateTime.UtcNow.AddDays(-5),
            TotalCredits = 9,
            EnrollmentDetails = new List<EnrollmentDetail>
            {
                new EnrollmentDetail { CourseId = courses[0].Id }, // Cálculo (T1)
                new EnrollmentDetail { CourseId = courses[2].Id }, // Algoritmos (T2)
                new EnrollmentDetail { CourseId = courses[6].Id }  // Bases de Datos (T4)
            }
        };
        context.Enrollments.Add(enrollment1);

        // Student 2: Mariana Ruiz (Courses: Álgebra [T1], Programación [T2], Redes [T5])
        var student2 = new Student
        {
            DocumentNumber = "1020789456",
            FullName = "Mariana Ruiz Gómez",
            Email = "mariana.ruiz@correo.com",
            Phone = "3159876543",
            RegistrationDate = DateTime.UtcNow.AddDays(-3)
        };
        context.Students.Add(student2);
        await context.SaveChangesAsync();

        var enrollment2 = new Enrollment
        {
            StudentId = student2.Id,
            EnrollmentDate = DateTime.UtcNow.AddDays(-3),
            TotalCredits = 9,
            EnrollmentDetails = new List<EnrollmentDetail>
            {
                new EnrollmentDetail { CourseId = courses[1].Id }, // Álgebra (T1)
                new EnrollmentDetail { CourseId = courses[3].Id }, // Programación (T2)
                new EnrollmentDetail { CourseId = courses[8].Id }  // Redes (T5)
            }
        };
        context.Enrollments.Add(enrollment2);

        // Student 3: Camilo Ochoa (Courses: Cálculo [T1], Física [T3], Bases de Datos [T4])
        // Notice: Camilo shares Cálculo with Juan Pérez and Bases de Datos with Juan Pérez!
        var student3 = new Student
        {
            DocumentNumber = "1032654987",
            FullName = "Camilo Andrés Ochoa",
            Email = "camilo.ochoa@correo.com",
            Phone = "3204567890",
            RegistrationDate = DateTime.UtcNow.AddDays(-1)
        };
        context.Students.Add(student3);
        await context.SaveChangesAsync();

        var enrollment3 = new Enrollment
        {
            StudentId = student3.Id,
            EnrollmentDate = DateTime.UtcNow.AddDays(-1),
            TotalCredits = 9,
            EnrollmentDetails = new List<EnrollmentDetail>
            {
                new EnrollmentDetail { CourseId = courses[0].Id }, // Cálculo (T1)
                new EnrollmentDetail { CourseId = courses[4].Id }, // Física (T3)
                new EnrollmentDetail { CourseId = courses[6].Id }  // Bases de Datos (T4)
            }
        };
        context.Enrollments.Add(enrollment3);

        await context.SaveChangesAsync();
    }
}
