-- =========================================================================
-- PRUEBA TÉCNICA INTER RAPIDÍSIMO - SISTEMA DE REGISTRO DE ESTUDIANTES
-- Script para MySQL / MariaDB
-- =========================================================================

CREATE DATABASE IF NOT EXISTS InterRapidisimoDb CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE InterRapidisimoDb;

-- 1. Eliminar tablas si existen en orden inverso por FK
DROP TABLE IF EXISTS EnrollmentDetails;
DROP TABLE IF EXISTS Enrollments;
DROP TABLE IF EXISTS Courses;
DROP TABLE IF EXISTS Teachers;
DROP TABLE IF EXISTS Students;

-- 2. Creación de Tabla Profesores (Hay 5 profesores en el sistema)
CREATE TABLE Teachers (
    Id INT AUTO_INCREMENT NOT NULL,
    FullName VARCHAR(150) NOT NULL,
    Email VARCHAR(150) NOT NULL,
    PRIMARY KEY (Id),
    UNIQUE KEY UQ_Teachers_Email (Email)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 3. Creación de Tabla Materias (Existen 10 materias, 3 créditos cada una, 2 por profesor)
CREATE TABLE Courses (
    Id INT AUTO_INCREMENT NOT NULL,
    Name VARCHAR(150) NOT NULL,
    Credits INT NOT NULL DEFAULT 3,
    TeacherId INT NOT NULL,
    PRIMARY KEY (Id),
    KEY FK_Courses_Teachers (TeacherId),
    CONSTRAINT FK_Courses_Teachers FOREIGN KEY (TeacherId) REFERENCES Teachers (Id) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 4. Creación de Tabla Estudiantes
CREATE TABLE Students (
    Id INT AUTO_INCREMENT NOT NULL,
    DocumentNumber VARCHAR(20) NOT NULL,
    FullName VARCHAR(150) NOT NULL,
    Email VARCHAR(150) NOT NULL,
    Phone VARCHAR(20) NULL,
    RegistrationDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (Id),
    UNIQUE KEY UQ_Students_Document (DocumentNumber),
    UNIQUE KEY UQ_Students_Email (Email)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 5. Creación de Tabla Matrícula / Registro de Créditos (9 créditos por matrícula)
CREATE TABLE Enrollments (
    Id INT AUTO_INCREMENT NOT NULL,
    StudentId INT NOT NULL,
    EnrollmentDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    TotalCredits INT NOT NULL DEFAULT 9,
    PRIMARY KEY (Id),
    KEY FK_Enrollments_Students (StudentId),
    CONSTRAINT FK_Enrollments_Students FOREIGN KEY (StudentId) REFERENCES Students (Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 6. Creación de Tabla Detalle de Matrícula (Cada estudiante matricula exactamente 3 materias)
CREATE TABLE EnrollmentDetails (
    Id INT AUTO_INCREMENT NOT NULL,
    EnrollmentId INT NOT NULL,
    CourseId INT NOT NULL,
    PRIMARY KEY (Id),
    KEY FK_EnrollmentDetails_Enrollments (EnrollmentId),
    KEY FK_EnrollmentDetails_Courses (CourseId),
    CONSTRAINT FK_EnrollmentDetails_Enrollments FOREIGN KEY (EnrollmentId) REFERENCES Enrollments (Id) ON DELETE CASCADE,
    CONSTRAINT FK_EnrollmentDetails_Courses FOREIGN KEY (CourseId) REFERENCES Courses (Id) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- =========================================================================
-- CARGA DE DATOS INICIALES (SEED DATA)
-- =========================================================================

-- A. Insertar 5 Profesores
INSERT INTO Teachers (FullName, Email) VALUES
('Dr. Carlos Mendoza', 'carlos.mendoza@interrapidisimo.edu.co'),
('Ing. Laura Gómez', 'laura.gomez@interrapidisimo.edu.co'),
('Dr. Roberto Silva', 'roberto.silva@interrapidisimo.edu.co'),
('Dra. Diana Torres', 'diana.torres@interrapidisimo.edu.co'),
('Mg. Andrés Morales', 'andres.morales@interrapidisimo.edu.co');

-- B. Insertar 10 Materias (2 por cada profesor, 3 créditos cada una)
INSERT INTO Courses (Name, Credits, TeacherId) VALUES
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

-- C. Insertar Estudiantes de Ejemplo
INSERT INTO Students (DocumentNumber, FullName, Email, Phone, RegistrationDate) VALUES
('1018456789', 'Juan Pablo Pérez', 'juan.perez@correo.com', '3101234567', DATE_SUB(NOW(), INTERVAL 5 DAY)),
('1020789456', 'Mariana Ruiz Gómez', 'mariana.ruiz@correo.com', '3159876543', DATE_SUB(NOW(), INTERVAL 3 DAY)),
('1032654987', 'Camilo Andrés Ochoa', 'camilo.ochoa@correo.com', '3204567890', DATE_SUB(NOW(), INTERVAL 1 DAY));

-- D. Matrícula de Juan Pablo Pérez (Materias: Cálculo [1], Algoritmos [3], Bases de Datos [7])
INSERT INTO Enrollments (StudentId, EnrollmentDate, TotalCredits) VALUES (1, DATE_SUB(NOW(), INTERVAL 5 DAY), 9);
INSERT INTO EnrollmentDetails (EnrollmentId, CourseId) VALUES 
(1, 1),
(1, 3),
(1, 7);

-- E. Matrícula de Mariana Ruiz (Materias: Álgebra [2], Programación [4], Redes [9])
INSERT INTO Enrollments (StudentId, EnrollmentDate, TotalCredits) VALUES (2, DATE_SUB(NOW(), INTERVAL 3 DAY), 9);
INSERT INTO EnrollmentDetails (EnrollmentId, CourseId) VALUES 
(2, 2),
(2, 4),
(2, 9);

-- F. Matrícula de Camilo Andrés Ochoa (Materias: Cálculo [1], Física [5], Bases de Datos [7])
INSERT INTO Enrollments (StudentId, EnrollmentDate, TotalCredits) VALUES (3, DATE_SUB(NOW(), INTERVAL 1 DAY), 9);
INSERT INTO EnrollmentDetails (EnrollmentId, CourseId) VALUES 
(3, 1),
(3, 5),
(3, 7);

-- =========================================================================
-- CONSULTA DE COMPROBACIÓN: REQUERIMIENTO 9 (COMPAÑEROS DE CLASE)
-- =========================================================================
-- Consulta que retorna solo los nombres de los compañeros con quienes comparte cada clase el estudiante 1 (Juan Pablo Pérez):
SELECT 
    c.Name AS Materia,
    t.FullName AS Profesor,
    classmate.FullName AS CompaneroDeClase
FROM Enrollments my_e
JOIN EnrollmentDetails my_ed ON my_e.Id = my_ed.EnrollmentId
JOIN Courses c ON my_ed.CourseId = c.Id
JOIN Teachers t ON c.TeacherId = t.Id
LEFT JOIN EnrollmentDetails other_ed ON other_ed.CourseId = c.Id
LEFT JOIN Enrollments other_e ON other_ed.EnrollmentId = other_e.Id AND other_e.StudentId <> my_e.StudentId
LEFT JOIN Students classmate ON other_e.StudentId = classmate.Id
WHERE my_e.StudentId = 1
ORDER BY c.Name, classmate.FullName;
