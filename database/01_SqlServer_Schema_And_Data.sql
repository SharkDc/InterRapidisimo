-- =========================================================================
-- PRUEBA TÉCNICA INTER RAPIDÍSIMO - SISTEMA DE REGISTRO DE ESTUDIANTES
-- Script para Microsoft SQL Server
-- =========================================================================

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'InterRapidisimoDb')
BEGIN
    CREATE DATABASE InterRapidisimoDb;
END
GO

USE InterRapidisimoDb;
GO

-- 1. Eliminar tablas si existen en orden inverso por FK
IF OBJECT_ID('dbo.EnrollmentDetails', 'U') IS NOT NULL DROP TABLE dbo.EnrollmentDetails;
IF OBJECT_ID('dbo.Enrollments', 'U') IS NOT NULL DROP TABLE dbo.Enrollments;
IF OBJECT_ID('dbo.Courses', 'U') IS NOT NULL DROP TABLE dbo.Courses;
IF OBJECT_ID('dbo.Teachers', 'U') IS NOT NULL DROP TABLE dbo.Teachers;
IF OBJECT_ID('dbo.Students', 'U') IS NOT NULL DROP TABLE dbo.Students;
GO

-- 2. Creación de Tabla Profesores (Hay 5 profesores en el sistema)
CREATE TABLE dbo.Teachers (
    Id INT IDENTITY(1,1) NOT NULL,
    FullName NVARCHAR(150) NOT NULL,
    Email NVARCHAR(150) NOT NULL,
    CONSTRAINT PK_Teachers PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_Teachers_Email UNIQUE (Email)
);
GO

-- 3. Creación de Tabla Materias (Existen 10 materias, 3 créditos cada una, 2 por profesor)
CREATE TABLE dbo.Courses (
    Id INT IDENTITY(1,1) NOT NULL,
    Name NVARCHAR(150) NOT NULL,
    Credits INT NOT NULL CONSTRAINT DF_Courses_Credits DEFAULT (3),
    TeacherId INT NOT NULL,
    CONSTRAINT PK_Courses PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_Courses_Teachers FOREIGN KEY (TeacherId) REFERENCES dbo.Teachers(Id)
);
GO

-- 4. Creación de Tabla Estudiantes
CREATE TABLE dbo.Students (
    Id INT IDENTITY(1,1) NOT NULL,
    DocumentNumber NVARCHAR(20) NOT NULL,
    FullName NVARCHAR(150) NOT NULL,
    Email NVARCHAR(150) NOT NULL,
    Phone NVARCHAR(20) NULL,
    RegistrationDate DATETIME2 NOT NULL CONSTRAINT DF_Students_RegDate DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_Students PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_Students_Document UNIQUE (DocumentNumber),
    CONSTRAINT UQ_Students_Email UNIQUE (Email)
);
GO

-- 5. Creación de Tabla Matrícula / Registro de Créditos (9 créditos por matrícula)
CREATE TABLE dbo.Enrollments (
    Id INT IDENTITY(1,1) NOT NULL,
    StudentId INT NOT NULL,
    EnrollmentDate DATETIME2 NOT NULL CONSTRAINT DF_Enrollments_Date DEFAULT (SYSUTCDATETIME()),
    TotalCredits INT NOT NULL CONSTRAINT DF_Enrollments_Credits DEFAULT (9),
    CONSTRAINT PK_Enrollments PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_Enrollments_Students FOREIGN KEY (StudentId) REFERENCES dbo.Students(Id) ON DELETE CASCADE
);
GO

-- 6. Creación de Tabla Detalle de Matrícula (Cada estudiante matricula exactamente 3 materias)
CREATE TABLE dbo.EnrollmentDetails (
    Id INT IDENTITY(1,1) NOT NULL,
    EnrollmentId INT NOT NULL,
    CourseId INT NOT NULL,
    CONSTRAINT PK_EnrollmentDetails PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_EnrollmentDetails_Enrollments FOREIGN KEY (EnrollmentId) REFERENCES dbo.Enrollments(Id) ON DELETE CASCADE,
    CONSTRAINT FK_EnrollmentDetails_Courses FOREIGN KEY (CourseId) REFERENCES dbo.Courses(Id)
);
GO

-- =========================================================================
-- CARGA DE DATOS INICIALES (SEED DATA)
-- =========================================================================

-- A. Insertar 5 Profesores
INSERT INTO dbo.Teachers (FullName, Email) VALUES
('Dr. Carlos Mendoza', 'carlos.mendoza@interrapidisimo.edu.co'),
('Ing. Laura Gómez', 'laura.gomez@interrapidisimo.edu.co'),
('Dr. Roberto Silva', 'roberto.silva@interrapidisimo.edu.co'),
('Dra. Diana Torres', 'diana.torres@interrapidisimo.edu.co'),
('Mg. Andrés Morales', 'andres.morales@interrapidisimo.edu.co');
GO

-- B. Insertar 10 Materias (2 por cada profesor, 3 créditos cada una)
INSERT INTO dbo.Courses (Name, Credits, TeacherId) VALUES
('Cálculo Diferencial', 3, 1),
('Álgebra Lineal', 3, 1),
('Algoritmos y Estructuras de Datos', 3, 2),
('Programación Orientada a Objetos', 3, 2),
('Física Mecánica', 3, 3),
('Electromagnetismo', 3, 3),
('Bases de Datos Relacionales', 3, 4),
('Arquitectura de Software', 3, 4),
('Redes de Computadores', 3, 5),
('Ingeniería de Requisitos', 3, 5);
GO

-- C. Insertar Estudiantes de Ejemplo
INSERT INTO dbo.Students (DocumentNumber, FullName, Email, Phone, RegistrationDate) VALUES
('1018456789', 'Juan Pablo Pérez', 'juan.perez@correo.com', '3101234567', DATEADD(DAY, -5, SYSUTCDATETIME())),
('1020789456', 'Mariana Ruiz Gómez', 'mariana.ruiz@correo.com', '3159876543', DATEADD(DAY, -3, SYSUTCDATETIME())),
('1032654987', 'Camilo Andrés Ochoa', 'camilo.ochoa@correo.com', '3204567890', DATEADD(DAY, -1, SYSUTCDATETIME()));
GO

-- D. Matrícula de Juan Pablo Pérez (Materias: Cálculo [1], Algoritmos [3], Bases de Datos [7]) - 3 Profesores diferentes (1, 2, 4)
INSERT INTO dbo.Enrollments (StudentId, EnrollmentDate, TotalCredits) VALUES (1, DATEADD(DAY, -5, SYSUTCDATETIME()), 9);
INSERT INTO dbo.EnrollmentDetails (EnrollmentId, CourseId) VALUES 
(1, 1),
(1, 3),
(1, 7);

-- E. Matrícula de Mariana Ruiz (Materias: Álgebra [2], Programación [4], Redes [9]) - 3 Profesores diferentes (1, 2, 5)
INSERT INTO dbo.Enrollments (StudentId, EnrollmentDate, TotalCredits) VALUES (2, DATEADD(DAY, -3, SYSUTCDATETIME()), 9);
INSERT INTO dbo.EnrollmentDetails (EnrollmentId, CourseId) VALUES 
(2, 2),
(2, 4),
(2, 9);

-- F. Matrícula de Camilo Andrés Ochoa (Materias: Cálculo [1], Física [5], Bases de Datos [7]) - Comparte materias con Juan Pablo
INSERT INTO dbo.Enrollments (StudentId, EnrollmentDate, TotalCredits) VALUES (3, DATEADD(DAY, -1, SYSUTCDATETIME()), 9);
INSERT INTO dbo.EnrollmentDetails (EnrollmentId, CourseId) VALUES 
(3, 1),
(3, 5),
(3, 7);
GO

-- =========================================================================
-- CONSULTA DE COMPROBACIÓN: REQUERIMIENTO 9 (COMPAÑEROS DE CLASE)
-- =========================================================================
-- Consulta que retorna solo los nombres de los compañeros con quienes comparte cada clase el estudiante 1 (Juan Pablo Pérez):
SELECT 
    c.Name AS Materia,
    t.FullName AS Profesor,
    classmate.FullName AS CompaneroDeClase
FROM dbo.Enrollments my_e
JOIN dbo.EnrollmentDetails my_ed ON my_e.Id = my_ed.EnrollmentId
JOIN dbo.Courses c ON my_ed.CourseId = c.Id
JOIN dbo.Teachers t ON c.TeacherId = t.Id
LEFT JOIN dbo.EnrollmentDetails other_ed ON other_ed.CourseId = c.Id
LEFT JOIN dbo.Enrollments other_e ON other_ed.EnrollmentId = other_e.Id AND other_e.StudentId <> my_e.StudentId
LEFT JOIN dbo.Students classmate ON other_e.StudentId = classmate.Id
WHERE my_e.StudentId = 1
ORDER BY c.Name, classmate.FullName;
GO
