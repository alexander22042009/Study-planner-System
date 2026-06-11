using Microsoft.AspNetCore.Mvc;
using Moq;
using StudyPlanner.API.Controllers;
using StudyPlanner.Core.Constants;
using StudyPlanner.Core.DTOs.Subjects;
using StudyPlanner.Core.Interfaces.Services;
using StudyPlanner.Core.Models.Common;
using StudyPlanner.Tests.Helpers;

namespace StudyPlanner.Tests.Controllers;

[TestFixture]
public class SubjectsControllerTests
{
    private Mock<ISubjectService> _service = null!;
    private SubjectsController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _service = new Mock<ISubjectService>();
        _controller = new SubjectsController(_service.Object);
        ControllerTestHelper.SetUser(_controller, "user-1", Roles.Student);
    }

    [Test]
    public async Task GetById_ReturnsOkWithApiResponse()
    {
        _service.Setup(s => s.GetByIdAsync(1, "user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SubjectDetailsDto { Id = 1, Name = "Math" });

        var result = await _controller.GetById(1, CancellationToken.None) as OkObjectResult;

        Assert.That(result, Is.Not.Null);
        var response = result!.Value as ApiResponse<SubjectDetailsDto>;
        Assert.That(response!.Success, Is.True);
        Assert.That(response.Data!.Name, Is.EqualTo("Math"));
    }

    [Test]
    public async Task Create_ReturnsCreatedResult()
    {
        _service.Setup(s => s.CreateAsync(It.IsAny<CreateSubjectDto>(), "user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SubjectDetailsDto { Id = 2, Name = "History" });

        var result = await _controller.Create(new CreateSubjectDto { Name = "History", Color = "#000000" }, CancellationToken.None) as CreatedAtActionResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.ActionName, Is.EqualTo(nameof(SubjectsController.GetById)));
    }
}
