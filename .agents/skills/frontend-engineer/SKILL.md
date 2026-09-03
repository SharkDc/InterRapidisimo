---
name: frontend-engineer
description: "Desarrollo de componentes Standalone en Angular 21, servicios reactivos HTTP, validaciones dinámicas en cliente y estilos corporativos para Inter Rapidísimo."
---

# Habilidad: Ingeniero Frontend (Inter Rapidísimo)

Usa esta habilidad para implementar vistas, componentes interactivos, servicios HTTP y estilos basados en el sistema de diseño corporativo.

## Flujo para Crear un Componente Standalone

### 1. Definición del Componente (`.ts`)
```typescript
import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { StudentService } from '../../services/student.service';

@Component({
  selector: 'app-nuevo-componente',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './nuevo-componente.html',
  styleUrls: ['./nuevo-componente.css']
})
export class NuevoComponenteComponent implements OnInit {
  private readonly studentService = inject(StudentService);
  
  loading = false;
  errorMessage = '';

  ngOnInit(): void {
    // Inicialización reactiva
  }
}
```

### 2. Implementación de Template con Clases Reutilizables
Aprovechar las clases de `src/styles.css`:
- Contenedores: `.container`, `.card`
- Botones: `.btn .btn-primary`, `.btn .btn-secondary`, `.btn .btn-outline-primary`, `.btn .btn-danger`
- Insignias: `.badge .badge-primary`, `.badge .badge-success`, `.badge .badge-warning`, `.badge .badge-muted`
- Formularios: `.form-group`, `.form-label`, `.form-control`
- Alertas: `.alert .alert-danger`, `.alert .alert-success`, `.alert .alert-warning`

### 3. Registro en Rutas (`app.routes.ts`)
```typescript
{
  path: 'mi-ruta',
  loadComponent: () => import('./components/nuevo-componente/nuevo-componente').then(m => m.NuevoComponenteComponent),
  title: 'Mi Título | Inter Rapidísimo'
}
```

## Comandos de Verificación
```powershell
cd frontend/inter-rapidisimo-client
npm run build
```
