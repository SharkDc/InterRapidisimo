using MediatR;
using Microsoft.AspNetCore.Mvc;
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
    public async Task<ActionResult<List<StudentDto>>> GetAllStudents()
    {
        var result = await _sender.Send(new GetAllStudentsQuery());
        return Ok(result);
    }

    /// <summary>
    /// Obtiene el detalle de un estudiante por su ID.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<StudentDto>> GetStudentById(int id)
    {
        var result = await _sender.Send(new GetStudentByIdQuery(id));
        return Ok(result);
    }

    /// <summary>
    /// Requerimiento 9: El estudiante podrá ver sólo el nombre de los alumnos con quienes compartirá cada clase.
    /// </summary>
    [HttpGet("{id:int}/classmates")]
    public async Task<ActionResult<StudentClassmatesDto>> GetStudentClassmates(int id)
    {
        var result = await _sender.Send(new GetStudentClassmatesQuery(id));
        return Ok(result);
    }

    /// <summary>
    /// Requerimiento 1 & 7: Realizar un CRUD de registro en línea con validación de 3 materias y profesores distintos.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult> CreateStudent([FromBody] CreateStudentCommand command)
    {
        var studentId = await _sender.Send(command);
        return CreatedAtAction(nameof(GetStudentById), new { id = studentId }, new { id = studentId, message = "Estudiante registrado exitosamente." });
    }

    /// <summary>
    /// Actualiza la información y materias de un estudiante.
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult> UpdateStudent(int id, [FromBody] UpdateStudentCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest(new { success = false, message = "El ID en la ruta no coincide con el ID del cuerpo de la petición." });
        }

        await _sender.Send(command);
        return Ok(new { success = true, message = "Estudiante actualizado exitosamente." });
    }

    /// <summary>
    /// Elimina el registro de un estudiante y su matrícula académica.
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteStudent(int id)
    {
        await _sender.Send(new DeleteStudentCommand(id));
        return Ok(new { success = true, message = "Estudiante eliminado exitosamente." });
    }
}
