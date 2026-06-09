using StudyPlanner.Core.DTOs.Goals;
using StudyPlanner.Core.Models.Common;

namespace StudyPlanner.Core.Interfaces.Services;

public interface IGoalService
{
    Task<PagedResult<GoalListDto>> GetAllAsync(
        string userId,
        SearchQuery search,
        SortQuery sort,
        PaginationQuery pagination,
        CancellationToken cancellationToken = default);

    Task<GoalDetailsDto> GetByIdAsync(int id, string userId, CancellationToken cancellationToken = default);

    Task<GoalDetailsDto> CreateAsync(CreateGoalDto dto, string userId, CancellationToken cancellationToken = default);

    Task<GoalDetailsDto> UpdateAsync(int id, EditGoalDto dto, string userId, CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, string userId, CancellationToken cancellationToken = default);

    Task<GoalDetailsDto> UpdateProgressAsync(int id, UpdateGoalProgressDto dto, string userId, CancellationToken cancellationToken = default);

    Task<GoalDetailsDto> CompleteAsync(int id, string userId, CancellationToken cancellationToken = default);
}
