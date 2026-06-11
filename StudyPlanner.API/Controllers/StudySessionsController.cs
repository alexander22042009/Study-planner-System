using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyPlanner.Core.Constants;
using StudyPlanner.Core.DTOs.Sessions;
using StudyPlanner.Core.Interfaces.Services;
using StudyPlanner.Core.Models.Common;

namespace StudyPlanner.API.Controllers;

[Authorize(Roles = $"{Roles.Student},{Roles.Administrator}")]
public class StudySessionsController : BaseApiController
{
    private readonly IStudySessionService _sessionService;

    public StudySessionsController(IStudySessionService sessionService)
    {
        _sessionService = sessionService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<SessionListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = PaginationQuery.DefaultPageNumber,
        [FromQuery] int pageSize = PaginationQuery.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var result = await _sessionService.GetAllAsync(
            GetUserId(),
            new PaginationQuery { PageNumber = pageNumber, PageSize = pageSize },
            cancellationToken);

        return OkResponse(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<SessionDetailsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _sessionService.GetByIdAsync(id, GetUserId(), cancellationToken);
        return OkResponse(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SessionDetailsDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateSessionDto dto, CancellationToken cancellationToken)
    {
        var result = await _sessionService.CreateAsync(dto, GetUserId(), cancellationToken);
        return CreatedResponse(nameof(GetById), new { id = result.Id }, result, "Study session created successfully.");
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<SessionDetailsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(int id, [FromBody] EditSessionDto dto, CancellationToken cancellationToken)
    {
        var result = await _sessionService.UpdateAsync(id, dto, GetUserId(), cancellationToken);
        return OkResponse(result, "Study session updated successfully.");
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _sessionService.DeleteAsync(id, GetUserId(), cancellationToken);
        return Ok(ApiResponse<object>.Ok(new { }, "Study session deleted successfully."));
    }

    [HttpGet("weekly")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<SessionListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWeekly(
        [FromQuery] DateTime? weekStart,
        CancellationToken cancellationToken = default)
    {
        var result = await _sessionService.GetWeeklyAsync(GetUserId(), weekStart, cancellationToken);
        return OkResponse(result);
    }
}
