namespace StudyPlanner.Core.DTOs.Admin;

public class UserListDto
{
    public string Id { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public DateTime DateCreated { get; set; }

    public decimal TotalStudyHours { get; set; }

    public IEnumerable<string> Roles { get; set; } = [];
}
