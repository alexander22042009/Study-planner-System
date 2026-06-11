using System.ComponentModel.DataAnnotations;

namespace StudyPlanner.Core.DTOs.Goals;

public class AddGoalHoursDto
{
    [Required]
    [Range(0.1, 24)]
    public decimal HoursStudied { get; set; }
}
