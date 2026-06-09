using Microsoft.EntityFrameworkCore;
using StudyPlanner.Core.Models.Common;

namespace StudyPlanner.Infrastructure.Repositories;

internal static class QueryableExtensions
{
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        PaginationQuery pagination,
        CancellationToken cancellationToken = default)
    {
        pagination.Normalize();

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<T>.Create(items, pagination.PageNumber, pagination.PageSize, totalCount);
    }
}
