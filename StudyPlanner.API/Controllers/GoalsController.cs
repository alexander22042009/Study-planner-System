using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyPlanner.Core.Constants;
using StudyPlanner.Core.DTOs.Goals;
using StudyPlanner.Core.Enums;
using StudyPlanner.Core.Interfaces.Services;
using StudyPlanner.Core.Models.Common;

namespace StudyPlanner.API.Controllers;

[Authorize(Roles = $"{Roles.Student},{Roles.Administrator}")]
public class GoalsController : BaseApiController
{
    private readonly IGoalService _goalService;

    public GoalsController(IGoalService goalService)
    {
        _goalService = goalService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<GoalListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? searchTerm,
        [FromQuery] string? sortBy,
        [FromQuery] SortDirection sortDirection = SortDirection.Ascending,
        [FromQuery] int pageNumber = PaginationQuery.DefaultPageNumber,
        [FromQuery] int pageSize = PaginationQuery.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var result = await _goalService.GetAllAsync(
            GetUserId(),
            new SearchQuery { SearchTerm = searchTerm },
            new SortQuery { SortBy = sortBy, SortDirection = sortDirection },
            new PaginationQuery { PageNumber = pageNumber, PageSize = pageSize },
            cancellationToken);

        return OkResponse(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<GoalDetailsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _goalService.GetByIdAsync(id, GetUserId(), cancellationToken);
        return OkResponse(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<GoalDetailsDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateGoalDto dto, CancellationToken cancellationToken)
    {
        var result = await _goalService.CreateAsync(dto, GetUserId(), cancellationToken);
        return CreatedResponse(nameof(GetById), new { id = result.Id }, result, "Goal created successfully.");
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<GoalDetailsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(int id, [FromBody] EditGoalDto dto, CancellationToken cancellationToken)
    {
        var result = await _goalService.UpdateAsync(id, dto, GetUserId(), cancellationToken);
        return OkResponse(result, "Goal updated successfully.");
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _goalService.DeleteAsync(id, GetUserId(), cancellationToken);
        return Ok(ApiResponse<object>.Ok(new { }, "Goal deleted successfully."));
    }

    [HttpPut("{id:int}/progress")]
    [ProducesResponseType(typeof(ApiResponse<GoalDetailsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateProgress(int id, [FromBody] UpdateGoalProgressDto dto, CancellationToken cancellationToken)
    {
        var result = await _goalService.UpdateProgressAsync(id, dto, GetUserId(), cancellationToken);
        return OkResponse(result, "Goal progress updated successfully.");
    }

    [HttpPost("{id:int}/add-hours")]
    [ProducesResponseType(typeof(ApiResponse<GoalDetailsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AddHours(int id, [FromBody] AddGoalHoursDto dto, CancellationToken cancellationToken)
    {
        var result = await _goalService.AddHoursAsync(id, dto, GetUserId(), cancellationToken);
        return OkResponse(result, result.Status == GoalStatus.Completed
            ? "Goal completed! Target hours reached."
            : "Study hours added successfully.");
    }

    [HttpPost("{id:int}/complete")]
    [ProducesResponseType(typeof(ApiResponse<GoalDetailsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Complete(int id, CancellationToken cancellationToken)
    {
        var result = await _goalService.CompleteAsync(id, GetUserId(), cancellationToken);
        return OkResponse(result, "Goal completed successfully.");
    }
}
