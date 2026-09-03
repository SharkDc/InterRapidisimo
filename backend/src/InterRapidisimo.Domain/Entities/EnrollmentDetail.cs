namespace InterRapidisimo.Domain.Entities;

public class EnrollmentDetail
{
    public int Id { get; set; }
    public int EnrollmentId { get; set; }
    public Enrollment Enrollment { get; set; } = null!;

    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;
}
