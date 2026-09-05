# Prueba Técnica Inter Rapidísimo - Sistema de Registro de Estudiantes

Aplicación web cliente-servidor para el registro de estudiantes en un programa académico de créditos, desarrollada cumpliendo el 100% de los requisitos técnicos solicitados:
- **Backend**: .NET 10 con Arquitectura Limpia y patrón **CQRS** (Command Query Responsibility Segregation) con MediatR, Entity Framework Core y validación formal de formatos con **FluentValidation** (documento numérico, teléfono y correo electrónico).
- **Frontend**: **Angular 21** con arquitectura Standalone Components, servicios HTTP reactivos, soporte nativo de **Modo Claro / Modo Oscuro**, diseño responsivo de alto contraste (WCAG AAA) e identidad corporativa oficial de **Inter Rapidísimo**.
- **Base de Datos**: Scripts SQL listos para **SQL Server** y **MySQL**, además de persistencia automática con **SQLite** (y conmutador a SQL Server en `appsettings.json`) para ejecución inmediata sin dependencias complejas.

---

## 📋 Matriz de Cumplimiento de Requerimientos

| # | Requerimiento de la Prueba Técnica | Estado | Detalle de Implementación |
|---|---|:---:|---|
| **1** | CRUD para registro en línea | ✅ | Operaciones completas de Crear, Consultar, Actualizar y Eliminar estudiantes con sus asignaturas matriculadas (`StudentsController`). Validación de datos en entrada (documento, correo, teléfono). |
| **2** | Adhesión a un programa de créditos | ✅ | Cada estudiante registra una matrícula académica asociada al programa de créditos institucionales. |
| **3** | Existen 10 materias | ✅ | 10 asignaturas sembradas en la base de datos y cargadas dinámicamente desde el endpoint `GET /api/courses`. |
| **4** | Cada materia equivale a 3 créditos | ✅ | Modelo de datos con restricción inmutable: `Credits = 3` por materia. |
| **5** | El estudiante sólo podrá seleccionar 3 materias | ✅ | Validación estricta en Backend (`CreateStudentCommandValidator` y `CreateStudentCommandHandler`) y UI interactiva: exactamente 3 materias (9 créditos totales). |
| **6** | 5 profesores que dictan 2 materias cada uno | ✅ | 5 docentes precargados en el sistema, cada uno con exactamente 2 materias asociadas (10 materias en total). |
| **7** | No podrá tener clases con el mismo profesor | ✅ | **Regla de negocio garantizada:** Si el estudiante intenta matricular dos materias del mismo docente, el backend rechaza la petición con un error 400 descriptivo (`BusinessRuleException`) y la interfaz en Angular bloquea dinámicamente en tiempo real las materias en conflicto. |
| **8** | Cada estudiante puede ver en línea registros de otros estudiantes | ✅ | Vista general pública interactiva (`/estudiantes`) con búsqueda en tiempo real, resumen de materias, docentes, créditos en badge protegido y soporte de temas Claro/Oscuro. |
| **9** | El estudiante podrá ver sólo el nombre de los alumnos con quienes compartirá cada clase | ✅ | Endpoint `GET /api/students/{id}/classmates` y vista dedicada (`/estudiantes/:id/companeros`): muestra cada materia del estudiante y **únicamente la lista de nombres de sus compañeros de clase**, excluyendo cualquier otro dato personal y excluyéndose a sí mismo. |
| **10** | Entregables: Aplicación Web y Scripts MySql / SQL | ✅ | Aplicación web completa y scripts DDL + DML en `database/01_SqlServer_Schema_And_Data.sql` y `database/02_MySql_Schema_And_Data.sql`. |

---

## 🛡️ Estándar de Comunicación API: Cabeceras y Envoltorio `ApiResponse<T>`

Todas las operaciones REST del sistema implementan un contrato estandarizado tanto a nivel de cabeceras de trazabilidad de entrada como en la estructura de respuesta JSON de salida:

### 1. Cabeceras HTTP de Trazabilidad (Obligatorias en Invocación)
Tanto la API como el cliente Angular (mediante `requestHeadersInterceptor`) garantizan el envío y auditoría de:
- **`systemid`**: Identificador del canal o sistema emisor de la petición (por defecto `inter-rapidisimo-web`).
- **`uuid`**: Identificador único correlacional (GUID/UUIDv4) para trazabilidad distribuida extremo a extremo.
- **`timestamp`**: Marca de tiempo en formato estándar ISO-8601 UTC (`yyyy-MM-ddTHH:mm:ss.fffZ`).

El middleware del backend (`RequestHeadersMiddleware`) valida estrictamente la presencia y formato de estas cabeceras en todas las rutas `/api/*`, respondiendo con HTTP `400 Bad Request` si alguna falta o es inválida. Asimismo, mediante `RequiredHeadersOperationFilter`, Swagger/OpenAPI documenta estas cabeceras como parámetros obligatorios (`required: true`) en todos los endpoints, facilitando su consumo y prueba interactiva desde Swagger UI. En las respuestas, la API devuelve estas cabeceras con el prefijo `X-` (`X-System-Id`, `X-Correlation-Id`, `X-Timestamp`).

### 2. Formato Estandarizado de Respuesta (`ApiResponse<T>`)
Todas las respuestas HTTP (exitosas y de error) devuelven el siguiente envoltorio uniforme:
- **`estado`** (`bool`): `true` cuando la operación se completó exitosamente; `false` ante fallos de validación de reglas de negocio o excepciones internas.
- **`descripcion`** (`string`): Mensaje descriptivo con el resultado de la acción realizada o la explicación precisa del motivo del error.
- **`data`** (`T?`): Carga útil solicitada (objeto, arreglo o identificador). En caso de error o ausencia de payload, retorna `null`.

#### Ejemplo de Respuesta Exitosa (200 OK)
```json
{
  "estado": true,
  "descripcion": "Catálogo de materias y profesores obtenido exitosamente.",
  "data": [
    {
      "id": 1,
      "name": "Cálculo Diferencial",
      "credits": 3,
      "teacherId": 1,
      "teacherName": "Dr. Carlos Mendoza"
    }
  ]
}
```

#### Ejemplo de Respuesta de Error de Negocio (400 Bad Request)
```json
{
  "estado": false,
  "descripcion": "El estudiante no podrá tener clases con el mismo profesor. Se detectó conflicto con el docente 'Dr. Carlos Mendoza' en las materias: 'Cálculo Diferencial', 'Álgebra Lineal'.",
  "data": null
}
```

---

## 🏛️ Arquitectura del Sistema

```
InterRapidisimo/
├── docs/
│   ├── Diseno_Tecnico_InterRapidisimo.pdf   # Documento Oficial de Diseño Técnico de Arquitectura (DTA en PDF)
│   └── Diseno_Tecnico_Arquitectura.html     # Fuente HTML/CSS y diagramas vectoriales SVG del DTA
├── backend/
│   ├── InterRapidisimo.sln
│   ├── src/
│   │   ├── InterRapidisimo.Domain/          # Entidades (Teacher, Course, Student, Enrollment, EnrollmentDetail), Excepciones
│   │   ├── InterRapidisimo.Application/     # CQRS (Commands, Queries, DTOs, Validadores FluentValidation, Pipeline Behavior)
│   │   ├── InterRapidisimo.Infrastructure/  # EF Core DbContext, DataSeeder automático, Inyección de dependencias
│   │   └── InterRapidisimo.Api/             # Controladores REST, Middleware Global de Excepciones, Swagger/OpenAPI, CORS
│   └── tests/
│       └── InterRapidisimo.Tests/           # Suite de Pruebas Unitarias de reglas de negocio
├── frontend/
│   └── inter-rapidisimo-client/             # Aplicación Angular (Standalone, Navbar, StudentList, StudentForm, Classmates)
└── database/
    ├── 01_SqlServer_Schema_And_Data.sql     # Script SQL Server (DDL + DML + Consulta Req 9)
    └── 02_MySql_Schema_And_Data.sql         # Script MySQL (DDL + DML + Consulta Req 9)
```

---

## 🚀 Instrucciones de Ejecución

### Requisitos Previos
- .NET 10 SDK
- Node.js (v20+ o v24) y npm

---

### 1. Levantar el Backend (.NET 10 Web API)

Abre una terminal en la carpeta `backend`:

```powershell
cd backend
dotnet run --project src/InterRapidisimo.Api
```

- La API iniciará en: **`http://localhost:5000`** (o `https://localhost:7001`)
- Puedes explorar y probar todos los endpoints en la documentación interactiva de Swagger:
  👉 **`http://localhost:5000/swagger`**

> **Nota sobre Base de Datos:**
> Por defecto, la aplicación utiliza SQLite (`interrapidisimo.db`) e inicializa automáticamente los 5 profesores, las 10 materias y 3 estudiantes de muestra con matrículas válidas para que puedas probar la aplicación de inmediato sin requerir configuración adicional de servidores.
>
> Si deseas usar SQL Server, cambia en `backend/src/InterRapidisimo.Api/appsettings.json`:
> `"DatabaseProvider": "SqlServer"`

---

### 2. Levantar el Frontend (Angular)

Abre otra terminal en la carpeta `frontend/inter-rapidisimo-client`:

```powershell
cd frontend/inter-rapidisimo-client
npm start
```

- La aplicación abrirá en: **`http://localhost:4200`**

---

### 3. Ejecutar las Pruebas Unitarias Automatizadas

Para validar las reglas de negocio (máximo 3 materias, 9 créditos, profesores distintos obligatorios, compañeros de clase, etc.) y la validación de cabeceras de trazabilidad:

```powershell
cd backend
dotnet test
```

Resultado esperado:
```
Correctas! - Con error: 0, Superado: 23, Omitido: 0, Total: 23
```

---

## 💾 Scripts de Base de Datos

En la carpeta `database/` se encuentran los scripts solicitados:
- **`database/01_SqlServer_Schema_And_Data.sql`**: Script completo para Microsoft SQL Server con creación de base de datos, tablas, llaves foráneas, restricciones de unicidad, inserción de los 5 profesores y las 10 materias, más la consulta SQL para el requerimiento 9.
- **`database/02_MySql_Schema_And_Data.sql`**: Script idéntico optimizado para MySQL / MariaDB con motor InnoDB y juego de caracteres UTF8MB4.
