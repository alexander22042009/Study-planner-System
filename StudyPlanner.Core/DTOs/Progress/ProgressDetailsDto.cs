namespace StudyPlanner.Core.DTOs.Progress;

public class ProgressDetailsDto
{
    public int Id { get; set; }

    public DateTime StudyDate { get; set; }

    public decimal HoursStudied { get; set; }

    public string? Notes { get; set; }

    public int SubjectId { get; set; }

    public string SubjectName { get; set; } = string.Empty;
}
