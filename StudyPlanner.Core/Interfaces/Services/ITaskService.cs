using StudyPlanner.Core.DTOs.Tasks;
using StudyPlanner.Core.Models.Common;

namespace StudyPlanner.Core.Interfaces.Services;

public interface ITaskService
{
    Task<PagedResult<TaskListDto>> GetAllAsync(
        string userId,
        SearchQuery search,
        SortQuery sort,
        PaginationQuery pagination,
        CancellationToken cancellationToken = default);

    Task<TaskDetailsDto> GetByIdAsync(int id, string userId, CancellationToken cancellationToken = default);

    Task<TaskDetailsDto> CreateAsync(CreateTaskDto dto, string userId, CancellationToken cancellationToken = default);

    Task<TaskDetailsDto> UpdateAsync(int id, EditTaskDto dto, string userId, CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, string userId, CancellationToken cancellationToken = default);

    Task<TaskDetailsDto> CompleteAsync(int id, string userId, CancellationToken cancellationToken = default);

    Task<PagedResult<TaskListDto>> GetBySubjectAsync(
        int subjectId,
        string userId,
        PaginationQuery pagination,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TaskListDto>> GetUpcomingAsync(
        string userId,
        int daysAhead,
        CancellationToken cancellationToken = default);
}
