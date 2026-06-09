namespace StudyPlanner.Core.DTOs.Admin;

public class AdminStatisticsDto
{
    public int ActiveUsersLast30Days { get; set; }

    public int CompletedTasks { get; set; }

    public int PendingTasks { get; set; }

    public decimal AverageStudyHoursPerUser { get; set; }

    public int GoalsCompleted { get; set; }

    public int TotalAchievementPoints { get; set; }
}
