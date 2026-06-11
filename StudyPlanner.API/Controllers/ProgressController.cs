using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyPlanner.Core.Constants;
using StudyPlanner.Core.DTOs.Progress;
using StudyPlanner.Core.Interfaces.Services;
using StudyPlanner.Core.Models.Common;

namespace StudyPlanner.API.Controllers;

[Authorize(Roles = $"{Roles.Student},{Roles.Administrator}")]
public class ProgressController : BaseApiController
{
    private readonly IProgressService _progressService;

    public ProgressController(IProgressService progressService)
    {
        _progressService = progressService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ProgressListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = PaginationQuery.DefaultPageNumber,
        [FromQuery] int pageSize = PaginationQuery.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var result = await _progressService.GetAllAsync(
            GetUserId(),
            new PaginationQuery { PageNumber = pageNumber, PageSize = pageSize },
            cancellationToken);

        return OkResponse(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<ProgressDetailsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _progressService.GetByIdAsync(id, GetUserId(), cancellationToken);
        return OkResponse(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ProgressDetailsDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateProgressDto dto, CancellationToken cancellationToken)
    {
        var result = await _progressService.CreateAsync(dto, GetUserId(), cancellationToken);
        return CreatedResponse(nameof(GetById), new { id = result.Id }, result, "Progress log created successfully.");
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<ProgressDetailsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(int id, [FromBody] EditProgressDto dto, CancellationToken cancellationToken)
    {
        var result = await _progressService.UpdateAsync(id, dto, GetUserId(), cancellationToken);
        return OkResponse(result, "Progress log updated successfully.");
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _progressService.DeleteAsync(id, GetUserId(), cancellationToken);
        return Ok(ApiResponse<object>.Ok(new { }, "Progress log deleted successfully."));
    }

    [HttpGet("statistics")]
    [ProducesResponseType(typeof(ApiResponse<StatisticsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatistics(CancellationToken cancellationToken)
    {
        var result = await _progressService.GetStatisticsAsync(GetUserId(), cancellationToken);
        return OkResponse(result);
    }

    [HttpGet("weekly")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<WeeklyProgressDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWeeklyProgress(
        [FromQuery] DateTime? weekStart,
        CancellationToken cancellationToken = default)
    {
        var result = await _progressService.GetWeeklyProgressAsync(GetUserId(), weekStart, cancellationToken);
        return OkResponse(result);
    }

    [HttpGet("monthly")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<MonthlyProgressDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMonthlyProgress(
        [FromQuery] int? year,
        [FromQuery] int? month,
        CancellationToken cancellationToken = default)
    {
        var result = await _progressService.GetMonthlyProgressAsync(GetUserId(), year, month, cancellationToken);
        return OkResponse(result);
    }
}
