namespace StudyPlanner.Core.Entities;

public class Subject
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string Color { get; set; } = "#3498db";

    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    public string UserId { get; set; } = string.Empty;

    public ApplicationUser? User { get; set; }

    public ICollection<StudyTask> StudyTasks { get; set; } = new List<StudyTask>();

    public ICollection<StudySession> StudySessions { get; set; } = new List<StudySession>();
}
