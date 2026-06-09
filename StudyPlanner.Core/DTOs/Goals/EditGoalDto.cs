using System.ComponentModel.DataAnnotations;
using StudyPlanner.Core.Enums;

namespace StudyPlanner.Core.DTOs.Goals;

public class EditGoalDto
{
    [Required]
    [StringLength(150, MinimumLength = 2)]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required]
    [Range(0.1, 10000)]
    public decimal TargetHours { get; set; }

    [Required]
    public DateTime Deadline { get; set; }

    [Required]
    public GoalStatus Status { get; set; }
}
