using Microsoft.AspNetCore.Mvc;
using Moq;
using StudyPlanner.API.Controllers;
using StudyPlanner.Core.Constants;
using StudyPlanner.Core.DTOs.Admin;
using StudyPlanner.Core.Interfaces.Services;
using StudyPlanner.Core.Models.Common;
using StudyPlanner.Tests.Helpers;

namespace StudyPlanner.Tests.Controllers;

[TestFixture]
public class AdminControllerTests
{
    private Mock<IAdminService> _adminService = null!;
    private AdminController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _adminService = new Mock<IAdminService>();
        _controller = new AdminController(_adminService.Object);
        ControllerTestHelper.SetUser(_controller, "admin-1", Roles.Administrator);
    }

    [Test]
    public async Task GetReports_ReturnsAdminReport()
    {
        _adminService.Setup(s => s.GetReportsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AdminReportDto { TotalUsers = 10, TotalTasks = 50 });

        var result = await _controller.GetReports(CancellationToken.None) as OkObjectResult;
        var response = result!.Value as ApiResponse<AdminReportDto>;

        Assert.That(response!.Data!.TotalUsers, Is.EqualTo(10));
        Assert.That(response.Data.TotalTasks, Is.EqualTo(50));
    }

    [Test]
    public async Task DeleteUser_CallsService()
    {
        await _controller.DeleteUser("user-2", CancellationToken.None);

        _adminService.Verify(s => s.DeleteUserAsync("user-2", It.IsAny<CancellationToken>()), Times.Once);
    }
}
