using StudyPlanner.Core.Enums;

namespace StudyPlanner.Core.Entities;

public class StudyTask
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime Deadline { get; set; }

    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    public Enums.TaskStatus Status { get; set; } = Enums.TaskStatus.Pending;

    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedOn { get; set; }

    public int SubjectId { get; set; }

    public string UserId { get; set; } = string.Empty;

    public Subject? Subject { get; set; }

    public ApplicationUser? User { get; set; }
}
