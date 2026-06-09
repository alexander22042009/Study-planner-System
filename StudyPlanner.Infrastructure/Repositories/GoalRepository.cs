using Microsoft.EntityFrameworkCore;
using StudyPlanner.Core.Entities;
using StudyPlanner.Core.Enums;
using StudyPlanner.Core.Interfaces.Repositories;
using StudyPlanner.Core.Models.Common;
using StudyPlanner.Infrastructure.Data;

namespace StudyPlanner.Infrastructure.Repositories;

public class GoalRepository : GenericRepository<Goal>, IGoalRepository
{
    public GoalRepository(ApplicationDbContext context)
        : base(context)
    {
    }

    public async Task<Goal?> GetByIdForUserAsync(int id, string userId, CancellationToken cancellationToken = default) =>
        await DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == id && g.UserId == userId, cancellationToken);

    public async Task<PagedResult<Goal>> GetPagedForUserAsync(
        string userId,
        SearchQuery search,
        SortQuery sort,
        PaginationQuery pagination,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking().Where(g => g.UserId == userId);

        if (search.HasSearchTerm)
        {
            var term = search.GetNormalizedSearchTerm();
            query = query.Where(g =>
                g.Title.Contains(term) ||
                (g.Description != null && g.Description.Contains(term)));
        }

        query = ApplySort(query, sort);

        return await query.ToPagedResultAsync(pagination, cancellationToken);
    }

    public Task<bool> ExistsForUserAsync(int id, string userId, CancellationToken cancellationToken = default) =>
        DbSet.AnyAsync(g => g.Id == id && g.UserId == userId, cancellationToken);

    public Task<int> GetCompletedCountForUserAsync(string userId, CancellationToken cancellationToken = default) =>
        DbSet.CountAsync(g => g.UserId == userId && g.Status == GoalStatus.Completed, cancellationToken);

    private static IQueryable<Goal> ApplySort(IQueryable<Goal> query, SortQuery sort)
    {
        var sortBy = sort.SortBy?.ToLowerInvariant() ?? "deadline";

        return sortBy switch
        {
            "date" or "createdon" => sort.IsDescending
                ? query.OrderByDescending(g => g.Deadline)
                : query.OrderBy(g => g.Deadline),
            "completion" or "status" => sort.IsDescending
                ? query.OrderByDescending(g => g.Status)
                : query.OrderBy(g => g.Status),
            "title" => sort.IsDescending
                ? query.OrderByDescending(g => g.Title)
                : query.OrderBy(g => g.Title),
            _ => sort.IsDescending
                ? query.OrderByDescending(g => g.Deadline)
                : query.OrderBy(g => g.Deadline)
        };
    }
}
