# Guía y Reglas del Proyecto - Inter Rapidísimo

Este repositorio contiene la solución completa para la **Prueba Técnica de Inter Rapidísimo - Sistema de Registro de Estudiantes en Programa de Créditos**.

---

## 👥 Equipo de Agentes Especializados

El proyecto cuenta con 3 agentes especializados para abordar las distintas dimensiones de la aplicación:

| Rol | Agente / Subagente | Especialidad y Alcance |
|---|---|---|
| 🏛️ **Arquitecto de Software** | `architect_agent` | Supervisión de Clean Architecture, cumplimiento de los 10 requerimientos técnicos, contratos de API REST, consistencia de datos (SQLite/SQL Server/MySQL) y diseño global. |
| ⚙️ **Ingeniero Backend** | `backend_agent` | Implementación en .NET 10 (C# 13), CQRS con MediatR, FluentValidation, Entity Framework Core 10, Controladores API y pruebas automatizadas con xUnit. |
| 🎨 **Ingeniero Frontend** | `frontend_agent` | Implementación en Angular 21 (Standalone Components), TypeScript, RxJS, servicios reactivos, validación en UI (selector interactivo de 3 materias) y diseño corporativo Inter Rapidísimo. |

---

## 📋 Reglas Globales de Arquitectura y Cumplimiento

1. **Inmutabilidad de Materias**: Existen exactamente 10 materias en el sistema, cada una de 3 créditos fijos.
2. **Distribución Docente**: Existen exactamente 5 profesores, cada uno con 2 materias asignadas.
3. **Restricción de Matrícula**: El estudiante solo puede inscribir exactamente 3 materias (9 créditos) y **no puede repetir profesor**.
4. **Privacidad en Compañeros de Clase**: La vista de compañeros de clase expone **única y exclusivamente los nombres** de los otros alumnos matriculados en cada materia.
5. **Persistencia Dual**: Soporte para SQLite (por defecto en local) y scripts SQL listos para SQL Server y MySQL en `database/`.

---

## 🚀 Comandos de Referencia Rápida

- **Backend**:
  ```powershell
  cd backend
  dotnet run --project src/InterRapidisimo.Api
  dotnet test
  ```
- **Frontend**:
  ```powershell
  cd frontend/inter-rapidisimo-client
  npm start
  npm run build
  ```
