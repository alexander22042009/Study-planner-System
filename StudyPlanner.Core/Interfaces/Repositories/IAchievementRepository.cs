using StudyPlanner.Core.Entities;
using StudyPlanner.Core.Models.Common;

namespace StudyPlanner.Core.Interfaces.Repositories;

public interface IAchievementRepository : IGenericRepository<Achievement>
{
    Task<Achievement?> GetByIdForUserAsync(int id, string userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Achievement>> GetAllForUserAsync(string userId, CancellationToken cancellationToken = default);

    Task<PagedResult<Achievement>> GetPagedForUserAsync(
        string userId,
        PaginationQuery pagination,
        CancellationToken cancellationToken = default);

    Task<Achievement?> GetByTitleForUserAsync(string title, string userId, CancellationToken cancellationToken = default);

    Task<int> GetTotalPointsForUserAsync(string userId, CancellationToken cancellationToken = default);

    Task<bool> ExistsForUserAsync(int id, string userId, CancellationToken cancellationToken = default);
}
