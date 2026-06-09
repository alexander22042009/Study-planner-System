using StudyPlanner.Core.Entities;
using StudyPlanner.Core.Models.Common;

namespace StudyPlanner.Core.Interfaces.Repositories;

public interface ITaskRepository : IGenericRepository<StudyTask>
{
    Task<StudyTask?> GetByIdForUserAsync(int id, string userId, CancellationToken cancellationToken = default);

    Task<PagedResult<StudyTask>> GetPagedForUserAsync(
        string userId,
        SearchQuery search,
        SortQuery sort,
        PaginationQuery pagination,
        CancellationToken cancellationToken = default);

    Task<PagedResult<StudyTask>> GetBySubjectForUserAsync(
        int subjectId,
        string userId,
        PaginationQuery pagination,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StudyTask>> GetUpcomingForUserAsync(
        string userId,
        int daysAhead,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsForUserAsync(int id, string userId, CancellationToken cancellationToken = default);

    Task<int> GetCompletedCountForUserAsync(string userId, CancellationToken cancellationToken = default);

    Task<int> GetPendingCountForUserAsync(string userId, CancellationToken cancellationToken = default);
}
