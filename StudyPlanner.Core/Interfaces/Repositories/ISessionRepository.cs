using StudyPlanner.Core.Entities;
using StudyPlanner.Core.Models.Common;

namespace StudyPlanner.Core.Interfaces.Repositories;

public interface ISessionRepository : IGenericRepository<StudySession>
{
    Task<StudySession?> GetByIdForUserAsync(int id, string userId, CancellationToken cancellationToken = default);

    Task<PagedResult<StudySession>> GetPagedForUserAsync(
        string userId,
        PaginationQuery pagination,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StudySession>> GetWeeklyForUserAsync(
        string userId,
        DateTime weekStart,
        DateTime weekEnd,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsForUserAsync(int id, string userId, CancellationToken cancellationToken = default);

    Task<int> GetCountForUserAsync(string userId, CancellationToken cancellationToken = default);
}
