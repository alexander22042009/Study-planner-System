namespace StudyPlanner.Core.DTOs.Sessions;

public class SessionDetailsDto
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public int Duration { get; set; }

    public string? Notes { get; set; }

    public int SubjectId { get; set; }

    public string SubjectName { get; set; } = string.Empty;
}
