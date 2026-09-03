using InterRapidisimo.Application.Courses.DTOs;

namespace InterRapidisimo.Application.Students.DTOs;

public class StudentDto
{
    public int Id { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime RegistrationDate { get; set; }
    public int TotalCredits { get; set; }
    public List<CourseDto> Courses { get; set; } = new();
}
