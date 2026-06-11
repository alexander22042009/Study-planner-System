using Microsoft.AspNetCore.Mvc;
using Moq;
using StudyPlanner.API.Controllers;
using StudyPlanner.Core.DTOs.Auth;
using StudyPlanner.Core.Interfaces.Services;
using StudyPlanner.Core.Models.Common;

namespace StudyPlanner.Tests.Controllers;

[TestFixture]
public class AuthControllerTests
{
    private Mock<IAuthService> _authService = null!;
    private AuthController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _authService = new Mock<IAuthService>();
        _controller = new AuthController(_authService.Object);
    }

    [Test]
    public async Task Login_ReturnsOkWithTokens()
    {
        _authService.Setup(s => s.LoginAsync(It.IsAny<LoginDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthResponseDto { Email = "test@test.com", AccessToken = "token" });

        var result = await _controller.Login(new LoginDto { Email = "test@test.com", Password = "Pass@12345" }, CancellationToken.None) as OkObjectResult;

        Assert.That(result, Is.Not.Null);
        var response = result!.Value as ApiResponse<AuthResponseDto>;
        Assert.That(response!.Data!.AccessToken, Is.EqualTo("token"));
    }

    [Test]
    public async Task Register_CallsAuthService()
    {
        _authService.Setup(s => s.RegisterAsync(It.IsAny<RegisterDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthResponseDto { Email = "new@test.com" });

        await _controller.Register(new RegisterDto
        {
            FirstName = "A",
            LastName = "B",
            Email = "new@test.com",
            Password = "Pass@12345",
            ConfirmPassword = "Pass@12345"
        }, CancellationToken.None);

        _authService.Verify(s => s.RegisterAsync(It.IsAny<RegisterDto>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
