# Reglas de Desarrollo Frontend - Inter Rapidísimo (Angular 21 Standalone)

Estas reglas se aplican a todo desarrollo o modificación dentro de `frontend/inter-rapidisimo-client/`.

## Directrices del Agente Frontend

1. **Angular 21 Standalone**:
   - Todo componente debe ser `standalone: true`. No usar NgModules obsoletos.
   - Inyección de dependencias mediante `inject()` de `@angular/core`.
   - Servicios HTTP reactivos con `HttpClient` y tipado riguroso con TypeScript.

2. **Diseño e Identidad Corporativa**:
   - Utilizar exclusivamente las variables CSS de `src/styles.css` (Naranja `#FF5722`, Superficie oscura `#1E293B`, estados de éxito/error/advertencia).
   - Mantener componentes limpios, accesibles y con diseño responsive para dispositivos móviles y escritorio.

3. **Validación Reactiva de Reglas de Negocio en la Interfaz**:
   - Selector de materias con contador en tiempo real (`X / 3 materias`, `Y / 9 créditos`).
   - Detección inmediata de conflictos de profesor y bloqueo visual para prevenir que el usuario intente matricular dos asignaturas del mismo docente.
   - Vista de compañeros de clase (`student-classmates`): renderizar exclusivamente los nombres de los compañeros en cada asignatura.

4. **Comandos de Verificación**:
   - Iniciar desarrollo: `npm start`
   - Compilación: `npm run build`
   - Pruebas: `npm test`
