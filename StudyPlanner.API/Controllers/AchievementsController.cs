using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyPlanner.Core.Constants;
using StudyPlanner.Core.DTOs.Achievements;
using StudyPlanner.Core.Interfaces.Services;
using StudyPlanner.Core.Models.Common;

namespace StudyPlanner.API.Controllers;

[Authorize(Roles = $"{Roles.Student},{Roles.Administrator}")]
public class AchievementsController : BaseApiController
{
    private readonly IAchievementService _achievementService;

    public AchievementsController(IAchievementService achievementService)
    {
        _achievementService = achievementService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<AchievementListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = PaginationQuery.DefaultPageNumber,
        [FromQuery] int pageSize = PaginationQuery.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var result = await _achievementService.GetAllAsync(
            GetUserId(),
            new PaginationQuery { PageNumber = pageNumber, PageSize = pageSize },
            cancellationToken);

        return OkResponse(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<AchievementDetailsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _achievementService.GetByIdAsync(id, GetUserId(), cancellationToken);
        return OkResponse(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<AchievementDetailsDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateAchievementDto dto, CancellationToken cancellationToken)
    {
        var result = await _achievementService.CreateAsync(dto, GetUserId(), cancellationToken);
        return CreatedResponse(nameof(GetById), new { id = result.Id }, result, "Achievement created successfully.");
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<AchievementDetailsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(int id, [FromBody] EditAchievementDto dto, CancellationToken cancellationToken)
    {
        var result = await _achievementService.UpdateAsync(id, dto, GetUserId(), cancellationToken);
        return OkResponse(result, "Achievement updated successfully.");
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _achievementService.DeleteAsync(id, GetUserId(), cancellationToken);
        return Ok(ApiResponse<object>.Ok(new { }, "Achievement deleted successfully."));
    }

    [HttpGet("user")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<AchievementListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserAchievements(CancellationToken cancellationToken)
    {
        var result = await _achievementService.GetUserAchievementsAsync(GetUserId(), cancellationToken);
        return OkResponse(result);
    }

    [HttpPost("{id:int}/unlock")]
    [ProducesResponseType(typeof(ApiResponse<AchievementDetailsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Unlock(int id, CancellationToken cancellationToken)
    {
        var result = await _achievementService.UnlockAchievementAsync(id, GetUserId(), cancellationToken);
        return OkResponse(result, "Achievement unlocked successfully.");
    }
}
