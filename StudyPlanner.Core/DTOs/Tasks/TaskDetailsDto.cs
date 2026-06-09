using StudyPlanner.Core.Enums;

namespace StudyPlanner.Core.DTOs.Tasks;

public class TaskDetailsDto
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime Deadline { get; set; }

    public TaskPriority Priority { get; set; }

    public Enums.TaskStatus Status { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? CompletedOn { get; set; }

    public int SubjectId { get; set; }

    public string SubjectName { get; set; } = string.Empty;
}
