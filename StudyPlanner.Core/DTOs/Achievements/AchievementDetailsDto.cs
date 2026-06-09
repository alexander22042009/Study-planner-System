namespace StudyPlanner.Core.DTOs.Achievements;

public class AchievementDetailsDto
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int Points { get; set; }

    public DateTime? UnlockedDate { get; set; }

    public bool IsUnlocked { get; set; }
}
