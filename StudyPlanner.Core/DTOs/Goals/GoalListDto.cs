using StudyPlanner.Core.Enums;

namespace StudyPlanner.Core.DTOs.Goals;

public class GoalListDto
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public decimal TargetHours { get; set; }

    public decimal CurrentHours { get; set; }

    public DateTime Deadline { get; set; }

    public GoalStatus Status { get; set; }
}
