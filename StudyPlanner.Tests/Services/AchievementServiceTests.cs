using Microsoft.EntityFrameworkCore;
using Moq;
using StudyPlanner.Core.Entities;
using StudyPlanner.Core.Exceptions;
using StudyPlanner.Core.Interfaces.Repositories;
using StudyPlanner.Infrastructure.Data;
using StudyPlanner.Infrastructure.Services;
using StudyPlanner.Tests.Helpers;

namespace StudyPlanner.Tests.Services;

[TestFixture]
public class AchievementServiceTests
{
    private Mock<IAchievementRepository> _achievementRepository = null!;
    private Mock<ISessionRepository> _sessionRepository = null!;
    private Mock<ITaskRepository> _taskRepository = null!;
    private Mock<IProgressRepository> _progressRepository = null!;
    private Mock<IGoalRepository> _goalRepository = null!;
    private Mock<IUnitOfWork> _unitOfWork = null!;
    private ApplicationDbContext _context = null!;
    private AchievementService _service = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);

        _achievementRepository = new Mock<IAchievementRepository>();
        _sessionRepository = new Mock<ISessionRepository>();
        _taskRepository = new Mock<ITaskRepository>();
        _progressRepository = new Mock<IProgressRepository>();
        _goalRepository = new Mock<IGoalRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();

        _service = new AchievementService(
            _achievementRepository.Object,
            _sessionRepository.Object,
            _taskRepository.Object,
            _progressRepository.Object,
            _goalRepository.Object,
            _unitOfWork.Object,
            TestMapperFactory.Create(),
            _context);
    }

    [TearDown]
    public void TearDown() => _context.Dispose();

    [Test]
    public async Task UnlockAchievementAsync_SetsUnlockedDate()
    {
        var achievement = new Achievement { Id = 1, UserId = "user-1", Title = "Test", Points = 10 };
        _achievementRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(achievement);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _service.UnlockAchievementAsync(1, "user-1");

        Assert.That(result.IsUnlocked, Is.True);
        Assert.That(achievement.UnlockedDate, Is.Not.Null);
    }

    [Test]
    public void GetByIdAsync_WhenNotFound_ThrowsNotFoundException()
    {
        _achievementRepository.Setup(r => r.GetByIdForUserAsync(5, "user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Achievement?)null);

        Assert.ThrowsAsync<NotFoundException>(() => _service.GetByIdAsync(5, "user-1"));
    }
}
