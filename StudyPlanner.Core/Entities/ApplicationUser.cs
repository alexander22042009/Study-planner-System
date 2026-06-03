using Microsoft.AspNetCore.Identity;

namespace StudyPlanner.Core.Entities;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string? ProfilePictureUrl { get; set; }

    public DateTime DateCreated { get; set; } = DateTime.UtcNow;

    public decimal TotalStudyHours { get; set; }

    public ICollection<Subject> Subjects { get; set; } = new List<Subject>();
}
