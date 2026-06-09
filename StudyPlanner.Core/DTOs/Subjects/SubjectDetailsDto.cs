namespace StudyPlanner.Core.DTOs.Subjects;

public class SubjectDetailsDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string Color { get; set; } = string.Empty;

    public DateTime CreatedOn { get; set; }

    public int TaskCount { get; set; }

    public int SessionCount { get; set; }
}
