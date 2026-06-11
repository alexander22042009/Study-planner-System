using Moq;
using StudyPlanner.Core.DTOs.Sessions;
using StudyPlanner.Core.Exceptions;
using StudyPlanner.Core.Interfaces.Repositories;
using StudyPlanner.Core.Interfaces.Services;
using StudyPlanner.Infrastructure.Services;
using StudyPlanner.Tests.Helpers;

namespace StudyPlanner.Tests.Services;

[TestFixture]
public class StudySessionServiceTests
{
    private Mock<ISessionRepository> _sessionRepository = null!;
    private Mock<ISubjectRepository> _subjectRepository = null!;
    private Mock<IUnitOfWork> _unitOfWork = null!;
    private Mock<IAchievementService> _achievementService = null!;
    private StudySessionService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _sessionRepository = new Mock<ISessionRepository>();
        _subjectRepository = new Mock<ISubjectRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _achievementService = new Mock<IAchievementService>();
        _service = new StudySessionService(_sessionRepository.Object, _subjectRepository.Object, _unitOfWork.Object, TestMapperFactory.Create(), _achievementService.Object);
    }

    [Test]
    public void CreateAsync_WhenEndBeforeStart_ThrowsBadRequestException()
    {
        var dto = new CreateSessionDto
        {
            Title = "Session",
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddMinutes(-30),
            SubjectId = 1
        };

        Assert.ThrowsAsync<BadRequestException>(() => _service.CreateAsync(dto, "user-1"));
    }

    [Test]
    public void CreateAsync_WhenSubjectNotFound_ThrowsBadRequestException()
    {
        _subjectRepository.Setup(r => r.ExistsForUserAsync(1, "user-1", It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var dto = new CreateSessionDto
        {
            Title = "Session",
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddHours(1),
            SubjectId = 1
        };

        Assert.ThrowsAsync<BadRequestException>(() => _service.CreateAsync(dto, "user-1"));
    }
}
