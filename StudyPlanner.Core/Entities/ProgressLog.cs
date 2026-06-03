namespace StudyPlanner.Core.Entities;

public class ProgressLog
{
    public int Id { get; set; }

    public DateTime StudyDate { get; set; }

    public decimal HoursStudied { get; set; }

    public string? Notes { get; set; }

    public string UserId { get; set; } = string.Empty;

    public int SubjectId { get; set; }

    public ApplicationUser? User { get; set; }

    public Subject? Subject { get; set; }
}
