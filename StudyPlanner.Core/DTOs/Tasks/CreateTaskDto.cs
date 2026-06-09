using System.ComponentModel.DataAnnotations;
using StudyPlanner.Core.Enums;

namespace StudyPlanner.Core.DTOs.Tasks;

public class CreateTaskDto
{
    [Required]
    [StringLength(150, MinimumLength = 2)]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required]
    public DateTime Deadline { get; set; }

    [Required]
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    [Required]
    [Range(1, int.MaxValue)]
    public int SubjectId { get; set; }
}
