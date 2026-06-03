using StudyPlanner.Core.Enums;

namespace StudyPlanner.Core.Entities;

public class Goal
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal TargetHours { get; set; }

    public decimal CurrentHours { get; set; }

    public DateTime Deadline { get; set; }

    public GoalStatus Status { get; set; } = GoalStatus.NotStarted;

    public string UserId { get; set; } = string.Empty;

    public ApplicationUser? User { get; set; }
}
