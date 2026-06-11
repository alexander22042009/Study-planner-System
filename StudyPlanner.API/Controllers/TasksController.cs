using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyPlanner.Core.Constants;
using StudyPlanner.Core.DTOs.Tasks;
using StudyPlanner.Core.Enums;
using StudyPlanner.Core.Interfaces.Services;
using StudyPlanner.Core.Models.Common;

namespace StudyPlanner.API.Controllers;

[Authorize(Roles = $"{Roles.Student},{Roles.Administrator}")]
public class TasksController : BaseApiController
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<TaskListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? searchTerm,
        [FromQuery] string? sortBy,
        [FromQuery] SortDirection sortDirection = SortDirection.Ascending,
        [FromQuery] int pageNumber = PaginationQuery.DefaultPageNumber,
        [FromQuery] int pageSize = PaginationQuery.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var result = await _taskService.GetAllAsync(
            GetUserId(),
            new SearchQuery { SearchTerm = searchTerm },
            new SortQuery { SortBy = sortBy, SortDirection = sortDirection },
            new PaginationQuery { PageNumber = pageNumber, PageSize = pageSize },
            cancellationToken);

        return OkResponse(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<TaskDetailsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _taskService.GetByIdAsync(id, GetUserId(), cancellationToken);
        return OkResponse(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<TaskDetailsDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateTaskDto dto, CancellationToken cancellationToken)
    {
        var result = await _taskService.CreateAsync(dto, GetUserId(), cancellationToken);
        return CreatedResponse(nameof(GetById), new { id = result.Id }, result, "Task created successfully.");
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<TaskDetailsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(int id, [FromBody] EditTaskDto dto, CancellationToken cancellationToken)
    {
        var result = await _taskService.UpdateAsync(id, dto, GetUserId(), cancellationToken);
        return OkResponse(result, "Task updated successfully.");
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _taskService.DeleteAsync(id, GetUserId(), cancellationToken);
        return Ok(ApiResponse<object>.Ok(new { }, "Task deleted successfully."));
    }

    [HttpPost("{id:int}/complete")]
    [ProducesResponseType(typeof(ApiResponse<TaskDetailsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Complete(int id, CancellationToken cancellationToken)
    {
        var result = await _taskService.CompleteAsync(id, GetUserId(), cancellationToken);
        return OkResponse(result, "Task completed successfully.");
    }

    [HttpGet("subject/{subjectId:int}")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<TaskListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBySubject(
        int subjectId,
        [FromQuery] int pageNumber = PaginationQuery.DefaultPageNumber,
        [FromQuery] int pageSize = PaginationQuery.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var result = await _taskService.GetBySubjectAsync(
            subjectId,
            GetUserId(),
            new PaginationQuery { PageNumber = pageNumber, PageSize = pageSize },
            cancellationToken);

        return OkResponse(result);
    }

    [HttpGet("upcoming")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<TaskListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUpcoming(
        [FromQuery] int daysAhead = 7,
        CancellationToken cancellationToken = default)
    {
        var result = await _taskService.GetUpcomingAsync(GetUserId(), daysAhead, cancellationToken);
        return OkResponse(result);
    }
}
