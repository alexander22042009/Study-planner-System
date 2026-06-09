using System.ComponentModel.DataAnnotations;

namespace StudyPlanner.Core.DTOs.Goals;

public class UpdateGoalProgressDto
{
    [Required]
    [Range(0, 10000)]
    public decimal CurrentHours { get; set; }
}
