using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using StudyPlanner.Core.Exceptions;
using StudyPlanner.Core.Models.Common;

namespace StudyPlanner.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class BaseApiController : ControllerBase
{
    protected string GetUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedException("User is not authenticated.");

    protected IActionResult OkResponse<T>(T data, string? message = null) =>
        Ok(ApiResponse<T>.Ok(data, message));

    protected IActionResult CreatedResponse<T>(string actionName, object routeValues, T data, string? message = null) =>
        CreatedAtAction(actionName, routeValues, ApiResponse<T>.Ok(data, message));
}
