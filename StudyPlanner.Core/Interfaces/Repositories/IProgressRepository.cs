using StudyPlanner.Core.Entities;
using StudyPlanner.Core.Models.Common;

namespace StudyPlanner.Core.Interfaces.Repositories;

public interface IProgressRepository : IGenericRepository<ProgressLog>
{
    Task<ProgressLog?> GetByIdForUserAsync(int id, string userId, CancellationToken cancellationToken = default);

    Task<PagedResult<ProgressLog>> GetPagedForUserAsync(
        string userId,
        PaginationQuery pagination,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProgressLog>> GetWeeklyForUserAsync(
        string userId,
        DateTime weekStart,
        DateTime weekEnd,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProgressLog>> GetMonthlyForUserAsync(
        string userId,
        int year,
        int month,
        CancellationToken cancellationToken = default);

    Task<decimal> GetTotalHoursForUserAsync(string userId, CancellationToken cancellationToken = default);

    Task<bool> ExistsForUserAsync(int id, string userId, CancellationToken cancellationToken = default);
}
