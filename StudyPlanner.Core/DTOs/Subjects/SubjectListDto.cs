namespace StudyPlanner.Core.DTOs.Subjects;

public class SubjectListDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Color { get; set; } = string.Empty;

    public DateTime CreatedOn { get; set; }
}
