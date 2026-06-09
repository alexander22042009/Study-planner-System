using System.ComponentModel.DataAnnotations;
using StudyPlanner.Core.Enums;

namespace StudyPlanner.Core.DTOs.Tasks;

public class EditTaskDto
{
    [Required]
    [StringLength(150, MinimumLength = 2)]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required]
    public DateTime Deadline { get; set; }

    [Required]
    public TaskPriority Priority { get; set; }

    [Required]
    public Enums.TaskStatus Status { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int SubjectId { get; set; }
}
