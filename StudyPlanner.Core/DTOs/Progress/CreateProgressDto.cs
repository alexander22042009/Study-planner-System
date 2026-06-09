using System.ComponentModel.DataAnnotations;

namespace StudyPlanner.Core.DTOs.Progress;

public class CreateProgressDto
{
    [Required]
    public DateTime StudyDate { get; set; }

    [Required]
    [Range(0.1, 24)]
    public decimal HoursStudied { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int SubjectId { get; set; }
}
