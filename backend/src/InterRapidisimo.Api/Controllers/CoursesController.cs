using MediatR;
using Microsoft.AspNetCore.Mvc;
using InterRapidisimo.Application.Courses.DTOs;
using InterRapidisimo.Application.Courses.Queries;

namespace InterRapidisimo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CoursesController : ControllerBase
{
    private readonly ISender _sender;

    public CoursesController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Obtiene las 10 materias disponibles asociadas a sus respectivos 5 profesores.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<CourseDto>>> GetCourses()
    {
        var result = await _sender.Send(new GetAvailableCoursesQuery());
        return Ok(result);
    }
}
