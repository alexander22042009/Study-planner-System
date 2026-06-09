namespace StudyPlanner.Core.DTOs.Admin;

public class UserDetailsDto
{
    public string Id { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string? ProfilePictureUrl { get; set; }

    public DateTime DateCreated { get; set; }

    public decimal TotalStudyHours { get; set; }

    public IEnumerable<string> Roles { get; set; } = [];

    public int SubjectCount { get; set; }

    public int TaskCount { get; set; }

    public int GoalCount { get; set; }

    public int AchievementCount { get; set; }
}
