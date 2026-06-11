using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyPlanner.Core.Constants;
using StudyPlanner.Core.DTOs.Admin;
using StudyPlanner.Core.Interfaces.Services;
using StudyPlanner.Core.Models.Common;

namespace StudyPlanner.API.Controllers;

[Authorize(Roles = Roles.Administrator)]
[Route("api/[controller]")]
public class AdminController : BaseApiController
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet("users")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<UserListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsers(
        [FromQuery] int pageNumber = PaginationQuery.DefaultPageNumber,
        [FromQuery] int pageSize = PaginationQuery.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var result = await _adminService.GetUsersAsync(
            new PaginationQuery { PageNumber = pageNumber, PageSize = pageSize },
            cancellationToken);

        return OkResponse(result);
    }

    [HttpGet("users/{userId}")]
    [ProducesResponseType(typeof(ApiResponse<UserDetailsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserById(string userId, CancellationToken cancellationToken)
    {
        var result = await _adminService.GetUserByIdAsync(userId, cancellationToken);
        return OkResponse(result);
    }

    [HttpDelete("users/{userId}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteUser(string userId, CancellationToken cancellationToken)
    {
        await _adminService.DeleteUserAsync(userId, cancellationToken);
        return Ok(ApiResponse<object>.Ok(new { }, "User deleted successfully."));
    }

    [HttpGet("reports")]
    [ProducesResponseType(typeof(ApiResponse<AdminReportDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReports(CancellationToken cancellationToken)
    {
        var result = await _adminService.GetReportsAsync(cancellationToken);
        return OkResponse(result);
    }

    [HttpGet("statistics")]
    [ProducesResponseType(typeof(ApiResponse<AdminStatisticsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatistics(CancellationToken cancellationToken)
    {
        var result = await _adminService.GetStatisticsAsync(cancellationToken);
        return OkResponse(result);
    }
}
