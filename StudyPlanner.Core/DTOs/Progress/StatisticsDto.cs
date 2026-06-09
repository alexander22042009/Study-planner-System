namespace StudyPlanner.Core.DTOs.Progress;

public class StatisticsDto
{
    public int TotalSubjects { get; set; }

    public int CompletedTasks { get; set; }

    public int PendingTasks { get; set; }

    public decimal HoursStudied { get; set; }

    public int GoalsCompleted { get; set; }

    public int AchievementPoints { get; set; }

    public IReadOnlyList<WeeklyProgressDto> WeeklyActivity { get; set; } = [];

    public IReadOnlyList<MonthlyProgressDto> MonthlyActivity { get; set; } = [];
}
