using Moq;
using StudyPlanner.Core.DTOs.Goals;
using StudyPlanner.Core.Entities;
using StudyPlanner.Core.Enums;
using StudyPlanner.Core.Exceptions;
using StudyPlanner.Core.Interfaces.Repositories;
using StudyPlanner.Core.Interfaces.Services;
using StudyPlanner.Infrastructure.Services;
using StudyPlanner.Tests.Helpers;

namespace StudyPlanner.Tests.Services;

[TestFixture]
public class GoalServiceTests
{
    private Mock<IGoalRepository> _goalRepository = null!;
    private Mock<IUnitOfWork> _unitOfWork = null!;
    private Mock<IAchievementService> _achievementService = null!;
    private GoalService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _goalRepository = new Mock<IGoalRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _achievementService = new Mock<IAchievementService>();
        _service = new GoalService(_goalRepository.Object, _unitOfWork.Object, TestMapperFactory.Create(), _achievementService.Object);
    }

    [Test]
    public void CreateAsync_WhenDeadlineInPast_ThrowsBadRequestException()
    {
        var dto = new CreateGoalDto
        {
            Title = "Goal",
            TargetHours = 10,
            Deadline = DateTime.UtcNow.AddDays(-2)
        };

        Assert.ThrowsAsync<BadRequestException>(() => _service.CreateAsync(dto, "user-1"));
    }

    [Test]
    public async Task CompleteAsync_SetsGoalToCompleted()
    {
        var goal = new Goal { Id = 1, UserId = "user-1", Title = "G", TargetHours = 20, CurrentHours = 5, Deadline = DateTime.UtcNow.AddDays(5) };
        _goalRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(goal);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _service.CompleteAsync(1, "user-1");

        Assert.That(result.Status, Is.EqualTo(GoalStatus.Completed));
        Assert.That(goal.CurrentHours, Is.EqualTo(goal.TargetHours));
    }

    [Test]
    public void UpdateProgressAsync_WhenHoursExceedTarget_ThrowsBadRequestException()
    {
        var goal = new Goal { Id = 1, UserId = "user-1", TargetHours = 10, CurrentHours = 2, Deadline = DateTime.UtcNow.AddDays(3) };
        _goalRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(goal);

        Assert.ThrowsAsync<BadRequestException>(() =>
            _service.UpdateProgressAsync(1, new UpdateGoalProgressDto { CurrentHours = 15 }, "user-1"));
    }
}
