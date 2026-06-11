using Moq;
using StudyPlanner.Core.DTOs.Subjects;
using StudyPlanner.Core.Entities;
using StudyPlanner.Core.Exceptions;
using StudyPlanner.Core.Interfaces.Repositories;
using StudyPlanner.Core.Models.Common;
using StudyPlanner.Infrastructure.Services;
using StudyPlanner.Tests.Helpers;

namespace StudyPlanner.Tests.Services;

[TestFixture]
public class SubjectServiceTests
{
    private Mock<ISubjectRepository> _repository = null!;
    private Mock<IUnitOfWork> _unitOfWork = null!;
    private SubjectService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _repository = new Mock<ISubjectRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _service = new SubjectService(_repository.Object, _unitOfWork.Object, TestMapperFactory.Create());
    }

    [Test]
    public async Task GetByIdAsync_WhenNotFound_ThrowsNotFoundException()
    {
        _repository.Setup(r => r.GetByIdForUserAsync(1, "user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Subject?)null);

        Assert.ThrowsAsync<NotFoundException>(() => _service.GetByIdAsync(1, "user-1"));
    }

    [Test]
    public async Task CreateAsync_CreatesSubjectAndReturnsDto()
    {
        _repository.Setup(r => r.AddAsync(It.IsAny<Subject>(), It.IsAny<CancellationToken>()))
            .Callback<Subject, CancellationToken>((s, _) => s.Id = 5);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _service.CreateAsync(new CreateSubjectDto
        {
            Name = "Math",
            Color = "#ff0000"
        }, "user-1");

        Assert.That(result.Name, Is.EqualTo("Math"));
        _repository.Verify(r => r.AddAsync(It.Is<Subject>(s => s.UserId == "user-1"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task DeleteAsync_WhenNotExists_ThrowsNotFoundException()
    {
        _repository.Setup(r => r.ExistsForUserAsync(9, "user-1", It.IsAny<CancellationToken>())).ReturnsAsync(false);

        Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteAsync(9, "user-1"));
    }

    [Test]
    public async Task GetAllAsync_ReturnsPagedMappedResults()
    {
        var subjects = new List<Subject> { new() { Id = 1, Name = "Physics", Color = "#fff", UserId = "user-1" } };
        var paged = PagedResult<Subject>.Create(subjects, 1, 10, 1);

        _repository.Setup(r => r.GetPagedForUserAsync("user-1", It.IsAny<SearchQuery>(), It.IsAny<SortQuery>(), It.IsAny<PaginationQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(paged);

        var result = await _service.GetAllAsync("user-1", new SearchQuery(), new SortQuery(), new PaginationQuery());

        Assert.That(result.Items, Has.Count.EqualTo(1));
        Assert.That(result.Items[0].Name, Is.EqualTo("Physics"));
    }
}
