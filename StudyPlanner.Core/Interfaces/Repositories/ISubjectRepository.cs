using StudyPlanner.Core.Entities;
using StudyPlanner.Core.Models.Common;

namespace StudyPlanner.Core.Interfaces.Repositories;

public interface ISubjectRepository : IGenericRepository<Subject>
{
    Task<Subject?> GetByIdForUserAsync(int id, string userId, CancellationToken cancellationToken = default);

    Task<PagedResult<Subject>> GetPagedForUserAsync(
        string userId,
        SearchQuery search,
        SortQuery sort,
        PaginationQuery pagination,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsForUserAsync(int id, string userId, CancellationToken cancellationToken = default);
}
