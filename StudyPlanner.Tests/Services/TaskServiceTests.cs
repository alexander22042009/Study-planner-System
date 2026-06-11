using Moq;
using StudyPlanner.Core.DTOs.Tasks;
using StudyPlanner.Core.Entities;
using StudyPlanner.Core.Enums;
using StudyPlanner.Core.Exceptions;
using StudyPlanner.Core.Interfaces.Repositories;
using StudyPlanner.Core.Models.Common;
using StudyPlanner.Infrastructure.Services;
using StudyPlanner.Tests.Helpers;
using TaskStatus = StudyPlanner.Core.Enums.TaskStatus;

namespace StudyPlanner.Tests.Services;

[TestFixture]
public class TaskServiceTests
{
    private Mock<ITaskRepository> _taskRepository = null!;
    private Mock<ISubjectRepository> _subjectRepository = null!;
    private Mock<IUnitOfWork> _unitOfWork = null!;
    private TaskService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _taskRepository = new Mock<ITaskRepository>();
        _subjectRepository = new Mock<ISubjectRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _service = new TaskService(_taskRepository.Object, _subjectRepository.Object, _unitOfWork.Object, TestMapperFactory.Create());
    }

    [Test]
    public void CreateAsync_WhenDeadlineInPast_ThrowsBadRequestException()
    {
        var dto = new CreateTaskDto
        {
            Title = "Task",
            Deadline = DateTime.UtcNow.AddDays(-1),
            SubjectId = 1,
            Priority = TaskPriority.High
        };

        Assert.ThrowsAsync<BadRequestException>(() => _service.CreateAsync(dto, "user-1"));
    }

    [Test]
    public async Task CompleteAsync_SetsCompletedStatus()
    {
        var task = new StudyTask { Id = 1, UserId = "user-1", Title = "T", Status = TaskStatus.Pending, SubjectId = 1, Deadline = DateTime.UtcNow.AddDays(1) };
        _taskRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(task);
        _taskRepository.Setup(r => r.GetByIdForUserAsync(1, "user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StudyTask { Id = 1, UserId = "user-1", Title = "T", Status = TaskStatus.Completed, SubjectId = 1, Deadline = DateTime.UtcNow.AddDays(1), Subject = new Subject { Name = "Math" } });
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _service.CompleteAsync(1, "user-1");

        Assert.That(result.Status, Is.EqualTo(TaskStatus.Completed));
        Assert.That(task.Status, Is.EqualTo(TaskStatus.Completed));
        Assert.That(task.CompletedOn, Is.Not.Null);
    }

    [Test]
    public void GetUpcomingAsync_WhenDaysAheadInvalid_ThrowsBadRequestException()
    {
        Assert.ThrowsAsync<BadRequestException>(() => _service.GetUpcomingAsync("user-1", 0));
    }

    [Test]
    public async Task GetBySubjectAsync_WhenSubjectMissing_ThrowsBadRequestException()
    {
        _subjectRepository.Setup(r => r.ExistsForUserAsync(3, "user-1", It.IsAny<CancellationToken>())).ReturnsAsync(false);

        Assert.ThrowsAsync<BadRequestException>(() => _service.GetBySubjectAsync(3, "user-1", new PaginationQuery()));
    }
}
