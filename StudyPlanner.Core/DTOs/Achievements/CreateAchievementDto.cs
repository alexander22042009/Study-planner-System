using System.ComponentModel.DataAnnotations;

namespace StudyPlanner.Core.DTOs.Achievements;

public class CreateAchievementDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Title { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    [Range(1, 10000)]
    public int Points { get; set; }
}
