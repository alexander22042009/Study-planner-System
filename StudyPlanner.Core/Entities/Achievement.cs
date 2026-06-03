namespace StudyPlanner.Core.Entities;

public class Achievement
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int Points { get; set; }

    public DateTime? UnlockedDate { get; set; }

    public string UserId { get; set; } = string.Empty;

    public ApplicationUser? User { get; set; }
}
