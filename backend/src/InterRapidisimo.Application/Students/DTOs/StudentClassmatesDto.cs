namespace InterRapidisimo.Application.Students.DTOs;

public class StudentClassmatesDto
{
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public List<CourseClassmatesDto> Courses { get; set; } = new();
}

public class CourseClassmatesDto
{
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public List<string> Classmates { get; set; } = new();
}
