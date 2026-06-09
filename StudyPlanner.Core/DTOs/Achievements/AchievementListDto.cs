namespace StudyPlanner.Core.DTOs.Achievements;

public class AchievementListDto
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public int Points { get; set; }

    public DateTime? UnlockedDate { get; set; }

    public bool IsUnlocked { get; set; }
}
