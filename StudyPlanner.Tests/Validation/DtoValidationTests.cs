using System.ComponentModel.DataAnnotations;
using StudyPlanner.Core.DTOs.Auth;
using StudyPlanner.Core.DTOs.Subjects;
using StudyPlanner.Core.DTOs.Tasks;
using StudyPlanner.Core.Enums;

namespace StudyPlanner.Tests.Validation;

[TestFixture]
public class DtoValidationTests
{
    private static IList<ValidationResult> Validate(object model)
    {
        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, context, results, true);
        return results;
    }

    [Test]
    public void RegisterDto_WithInvalidEmail_FailsValidation()
    {
        var dto = new RegisterDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "invalid-email",
            Password = "Password@1",
            ConfirmPassword = "Password@1"
        };

        var results = Validate(dto);

        Assert.That(results, Is.Not.Empty);
    }

    [Test]
    public void LoginDto_WithEmptyPassword_FailsValidation()
    {
        var dto = new LoginDto { Email = "user@test.com", Password = "" };
        var results = Validate(dto);
        Assert.That(results, Is.Not.Empty);
    }

    [Test]
    public void CreateSubjectDto_WithValidData_PassesValidation()
    {
        var dto = new CreateSubjectDto { Name = "Biology", Color = "#aabbcc" };
        var results = Validate(dto);
        Assert.That(results, Is.Empty);
    }

    [Test]
    public void CreateTaskDto_WithInvalidColorSubjectId_FailsValidation()
    {
        var dto = new CreateTaskDto
        {
            Title = "T",
            Deadline = DateTime.UtcNow.AddDays(2),
            Priority = TaskPriority.Low,
            SubjectId = 0
        };

        var results = Validate(dto);
        Assert.That(results, Is.Not.Empty);
    }
}
