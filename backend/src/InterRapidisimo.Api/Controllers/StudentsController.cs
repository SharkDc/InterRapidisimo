using MediatR;
using Microsoft.AspNetCore.Mvc;
using InterRapidisimo.Application.Common.Models;
using InterRapidisimo.Application.Students.Commands;
using InterRapidisimo.Application.Students.DTOs;
using InterRapidisimo.Application.Students.Queries;

namespace InterRapidisimo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentsController : ControllerBase
{
    private readonly ISender _sender;

    public StudentsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Requerimiento 8: Permite consultar en línea los registros de todos los estudiantes.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<StudentDto>>>> GetAllStudents()
    {
        var result = await _sender.Send(new GetAllStudentsQuery());
        return Ok(ApiResponse.Success(result, "Listado de estudiantes obtenido exitosamente."));
    }

    /// <summary>
    /// Obtiene el detalle de un estudiante por su ID.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<StudentDto>>> GetStudentById(int id)
    {
        var result = await _sender.Send(new GetStudentByIdQuery(id));
        return Ok(ApiResponse.Success(result, "Expediente del estudiante obtenido exitosamente."));
    }

    /// <summary>
    /// Requerimiento 9: El estudiante podrá ver sólo el nombre de los alumnos con quienes compartirá cada clase.
    /// </summary>
    [HttpGet("{id:int}/classmates")]
    public async Task<ActionResult<ApiResponse<StudentClassmatesDto>>> GetStudentClassmates(int id)
    {
        var result = await _sender.Send(new GetStudentClassmatesQuery(id));
        return Ok(ApiResponse.Success(result, "Compañeros de clase obtenidos exitosamente."));
    }

    /// <summary>
    /// Requerimiento 1 & 7: Realizar un CRUD de registro en línea con validación de 3 materias y profesores distintos.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<object>>> CreateStudent([FromBody] CreateStudentCommand command)
    {
        var studentId = await _sender.Send(command);
        return CreatedAtAction(
            nameof(GetStudentById),
            new { id = studentId },
            ApiResponse.Success(new { id = studentId }, "Estudiante registrado exitosamente.")
        );
    }

    /// <summary>
    /// Actualiza la información y materias de un estudiante.
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateStudent(int id, [FromBody] UpdateStudentCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest(ApiResponse.Failure("El ID en la ruta no coincide con el ID del cuerpo de la petición."));
        }

        await _sender.Send(command);
        return Ok(ApiResponse.Success(true, "Estudiante actualizado exitosamente."));
    }

    /// <summary>
    /// Elimina el registro de un estudiante y su matrícula académica.
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteStudent(int id)
    {
        await _sender.Send(new DeleteStudentCommand(id));
        return Ok(ApiResponse.Success(true, "Estudiante eliminado exitosamente."));
    }
}
