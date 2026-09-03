---
name: project-architect
description: "Auditoría de arquitectura, revisión de impacto, validación de contratos API y gobernanza de Clean Architecture + CQRS para el proyecto Inter Rapidísimo."
---

# Habilidad: Arquitecto de Software (Inter Rapidísimo)

Usa esta habilidad para planificar, auditar y validar cambios estructurales en el sistema de registro de estudiantes de Inter Rapidísimo.

## Flujo de Auditoría de Cambios Arquitectónicos

### 1. Verificación de Matriz de Requisitos
Antes de autorizar cualquier refactorización o nueva funcionalidad, valida que no se viole ninguno de los 10 requerimientos:
- 10 materias fijas (`Credits = 3`).
- 5 profesores fijos (2 materias cada uno).
- Exactamente 3 materias por estudiante (9 créditos).
- Profesores distintos obligatorios.
- Consulta de compañeros solo expone nombres.
- Scripts SQL compatibles con SQL Server y MySQL.

### 2. Flujo de Revisión de Contratos API
Cuando se añada o modifique un endpoint:
1. Definir el DTO en `InterRapidisimo.Application/{Feature}/DTOs/`.
2. Crear el Command o Query correspondiente en `InterRapidisimo.Application/{Feature}/`.
3. Si es Command, crear el Validador con FluentValidation en la misma carpeta.
4. Exponer en el Controller correspondiente (`InterRapidisimo.Api/Controllers/`).
5. Actualizar la interface TypeScript en `frontend/inter-rapidisimo-client/src/app/models/`.
6. Actualizar el método reactivo en `frontend/inter-rapidisimo-client/src/app/services/`.

### 3. Checklist de Verificación de Salud del Sistema
Ejecuta la validación completa en 2 pasos:
```powershell
# 1. Validación Backend y Pruebas Unitarias
cd backend
dotnet test

# 2. Validación Frontend
cd ../frontend/inter-rapidisimo-client
npm run build
```
