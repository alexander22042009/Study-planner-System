using Microsoft.EntityFrameworkCore;
using StudyPlanner.Core.Entities;
using StudyPlanner.Core.Interfaces.Repositories;
using StudyPlanner.Core.Models.Common;
using StudyPlanner.Infrastructure.Data;

namespace StudyPlanner.Infrastructure.Repositories;

public class SubjectRepository : GenericRepository<Subject>, ISubjectRepository
{
    public SubjectRepository(ApplicationDbContext context)
        : base(context)
    {
    }

    public async Task<Subject?> GetByIdForUserAsync(int id, string userId, CancellationToken cancellationToken = default) =>
        await DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId, cancellationToken);

    public async Task<PagedResult<Subject>> GetPagedForUserAsync(
        string userId,
        SearchQuery search,
        SortQuery sort,
        PaginationQuery pagination,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking().Where(s => s.UserId == userId);

        if (search.HasSearchTerm)
        {
            var term = search.GetNormalizedSearchTerm();
            query = query.Where(s =>
                s.Name.Contains(term) ||
                (s.Description != null && s.Description.Contains(term)));
        }

        query = ApplySort(query, sort);

        return await query.ToPagedResultAsync(pagination, cancellationToken);
    }

    public Task<bool> ExistsForUserAsync(int id, string userId, CancellationToken cancellationToken = default) =>
        DbSet.AnyAsync(s => s.Id == id && s.UserId == userId, cancellationToken);

    private static IQueryable<Subject> ApplySort(IQueryable<Subject> query, SortQuery sort)
    {
        var sortBy = sort.SortBy?.ToLowerInvariant() ?? "date";

        return sortBy switch
        {
            "name" => sort.IsDescending
                ? query.OrderByDescending(s => s.Name)
                : query.OrderBy(s => s.Name),
            _ => sort.IsDescending
                ? query.OrderByDescending(s => s.CreatedOn)
                : query.OrderBy(s => s.CreatedOn)
        };
    }
}
