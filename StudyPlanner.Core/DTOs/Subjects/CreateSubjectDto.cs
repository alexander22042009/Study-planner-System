using System.ComponentModel.DataAnnotations;

namespace StudyPlanner.Core.DTOs.Subjects;

public class CreateSubjectDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    [StringLength(7, MinimumLength = 4)]
    [RegularExpression(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$")]
    public string Color { get; set; } = "#3498db";
}
