using StudyPlanner.Core.Entities;
using StudyPlanner.Core.Models.Common;

namespace StudyPlanner.Core.Interfaces.Repositories;

public interface IGoalRepository : IGenericRepository<Goal>
{
    Task<Goal?> GetByIdForUserAsync(int id, string userId, CancellationToken cancellationToken = default);

    Task<PagedResult<Goal>> GetPagedForUserAsync(
        string userId,
        SearchQuery search,
        SortQuery sort,
        PaginationQuery pagination,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsForUserAsync(int id, string userId, CancellationToken cancellationToken = default);

    Task<int> GetCompletedCountForUserAsync(string userId, CancellationToken cancellationToken = default);
}
