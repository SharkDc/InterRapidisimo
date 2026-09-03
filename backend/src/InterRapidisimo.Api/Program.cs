using InterRapidisimo.Api.Middlewares;
using InterRapidisimo.Application;
using InterRapidisimo.Infrastructure;
using InterRapidisimo.Infrastructure.Data;
using InterRapidisimo.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// 1. Inyección de dependencias de capas CQRS e Infraestructura
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// 2. Controladores
builder.Services.AddControllers();

// 3. Documentación OpenAPI / Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Inter Rapidísimo - API de Registro de Estudiantes (CQRS)",
        Version = "v1",
        Description = "API RESTful construida con .NET 10 y patrón CQRS para el registro de estudiantes, materias, profesores y cálculo de créditos."
    });
});

// 4. Política de CORS para la aplicación Angular
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "http://127.0.0.1:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// 5. Inicialización automática de Base de Datos y Carga de Datos Iniciales (5 Profesores, 10 Materias)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        await DbInitializer.SeedAsync(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al inicializar y sembrar la base de datos.");
    }
}

// 6. Pipeline HTTP
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Inter Rapidísimo API v1");
    });
}

app.UseCors("AllowAngular");

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.MapControllers();

app.Run();
