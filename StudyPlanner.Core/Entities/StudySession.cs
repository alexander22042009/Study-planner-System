namespace StudyPlanner.Core.Entities;

public class StudySession
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public int Duration { get; set; }

    public string? Notes { get; set; }

    public int SubjectId { get; set; }

    public string UserId { get; set; } = string.Empty;

    public Subject? Subject { get; set; }

    public ApplicationUser? User { get; set; }
}
