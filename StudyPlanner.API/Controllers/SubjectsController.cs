using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyPlanner.Core.Constants;
using StudyPlanner.Core.DTOs.Subjects;
using StudyPlanner.Core.Enums;
using StudyPlanner.Core.Interfaces.Services;
using StudyPlanner.Core.Models.Common;

namespace StudyPlanner.API.Controllers;

[Authorize(Roles = $"{Roles.Student},{Roles.Administrator}")]
public class SubjectsController : BaseApiController
{
    private readonly ISubjectService _subjectService;

    public SubjectsController(ISubjectService subjectService)
    {
        _subjectService = subjectService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<SubjectListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? searchTerm,
        [FromQuery] string? sortBy,
        [FromQuery] SortDirection sortDirection = SortDirection.Ascending,
        [FromQuery] int pageNumber = PaginationQuery.DefaultPageNumber,
        [FromQuery] int pageSize = PaginationQuery.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var result = await _subjectService.GetAllAsync(
            GetUserId(),
            new SearchQuery { SearchTerm = searchTerm },
            new SortQuery { SortBy = sortBy, SortDirection = sortDirection },
            new PaginationQuery { PageNumber = pageNumber, PageSize = pageSize },
            cancellationToken);

        return OkResponse(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<SubjectDetailsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _subjectService.GetByIdAsync(id, GetUserId(), cancellationToken);
        return OkResponse(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SubjectDetailsDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateSubjectDto dto, CancellationToken cancellationToken)
    {
        var result = await _subjectService.CreateAsync(dto, GetUserId(), cancellationToken);
        return CreatedResponse(nameof(GetById), new { id = result.Id }, result, "Subject created successfully.");
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<SubjectDetailsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(int id, [FromBody] EditSubjectDto dto, CancellationToken cancellationToken)
    {
        var result = await _subjectService.UpdateAsync(id, dto, GetUserId(), cancellationToken);
        return OkResponse(result, "Subject updated successfully.");
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _subjectService.DeleteAsync(id, GetUserId(), cancellationToken);
        return Ok(ApiResponse<object>.Ok(new { }, "Subject deleted successfully."));
    }
}
