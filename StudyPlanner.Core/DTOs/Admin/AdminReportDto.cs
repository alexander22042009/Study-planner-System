namespace StudyPlanner.Core.DTOs.Admin;

public class AdminReportDto
{
    public int TotalUsers { get; set; }

    public int TotalStudents { get; set; }

    public int TotalAdministrators { get; set; }

    public int TotalSubjects { get; set; }

    public int TotalTasks { get; set; }

    public int TotalSessions { get; set; }

    public int TotalGoals { get; set; }

    public int TotalAchievements { get; set; }

    public decimal TotalStudyHours { get; set; }

    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}
