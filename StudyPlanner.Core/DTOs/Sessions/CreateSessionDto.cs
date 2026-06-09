using System.ComponentModel.DataAnnotations;

namespace StudyPlanner.Core.DTOs.Sessions;

public class CreateSessionDto
{
    [Required]
    [StringLength(150, MinimumLength = 2)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public DateTime StartTime { get; set; }

    [Required]
    public DateTime EndTime { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int SubjectId { get; set; }
}
