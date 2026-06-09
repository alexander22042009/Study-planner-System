using Microsoft.EntityFrameworkCore;
using StudyPlanner.Core.Entities;
using StudyPlanner.Core.Enums;
using StudyPlanner.Core.Interfaces.Repositories;
using TaskStatus = StudyPlanner.Core.Enums.TaskStatus;
using StudyPlanner.Core.Models.Common;
using StudyPlanner.Infrastructure.Data;

namespace StudyPlanner.Infrastructure.Repositories;

public class TaskRepository : GenericRepository<StudyTask>, ITaskRepository
{
    public TaskRepository(ApplicationDbContext context)
        : base(context)
    {
    }

    public async Task<StudyTask?> GetByIdForUserAsync(int id, string userId, CancellationToken cancellationToken = default) =>
        await DbSet
            .AsNoTracking()
            .Include(t => t.Subject)
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId, cancellationToken);

    public async Task<PagedResult<StudyTask>> GetPagedForUserAsync(
        string userId,
        SearchQuery search,
        SortQuery sort,
        PaginationQuery pagination,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .AsNoTracking()
            .Include(t => t.Subject)
            .Where(t => t.UserId == userId);

        if (search.HasSearchTerm)
        {
            var term = search.GetNormalizedSearchTerm();
            query = query.Where(t =>
                t.Title.Contains(term) ||
                (t.Description != null && t.Description.Contains(term)));
        }

        query = ApplySort(query, sort);

        return await query.ToPagedResultAsync(pagination, cancellationToken);
    }

    public async Task<PagedResult<StudyTask>> GetBySubjectForUserAsync(
        int subjectId,
        string userId,
        PaginationQuery pagination,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .AsNoTracking()
            .Include(t => t.Subject)
            .Where(t => t.SubjectId == subjectId && t.UserId == userId)
            .OrderBy(t => t.Deadline);

        return await query.ToPagedResultAsync(pagination, cancellationToken);
    }

    public async Task<IReadOnlyList<StudyTask>> GetUpcomingForUserAsync(
        string userId,
        int daysAhead,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var endDate = now.AddDays(daysAhead);

        return await DbSet
            .AsNoTracking()
            .Include(t => t.Subject)
            .Where(t =>
                t.UserId == userId &&
                t.Status != TaskStatus.Completed &&
                t.Deadline >= now &&
                t.Deadline <= endDate)
            .OrderBy(t => t.Deadline)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsForUserAsync(int id, string userId, CancellationToken cancellationToken = default) =>
        DbSet.AnyAsync(t => t.Id == id && t.UserId == userId, cancellationToken);

    public Task<int> GetCompletedCountForUserAsync(string userId, CancellationToken cancellationToken = default) =>
        DbSet.CountAsync(t => t.UserId == userId && t.Status == TaskStatus.Completed, cancellationToken);

    public Task<int> GetPendingCountForUserAsync(string userId, CancellationToken cancellationToken = default) =>
        DbSet.CountAsync(t =>
            t.UserId == userId &&
            (t.Status == TaskStatus.Pending || t.Status == TaskStatus.InProgress),
            cancellationToken);

    private static IQueryable<StudyTask> ApplySort(IQueryable<StudyTask> query, SortQuery sort)
    {
        var sortBy = sort.SortBy?.ToLowerInvariant() ?? "deadline";

        return sortBy switch
        {
            "priority" => sort.IsDescending
                ? query.OrderByDescending(t => t.Priority)
                : query.OrderBy(t => t.Priority),
            "date" or "createdon" => sort.IsDescending
                ? query.OrderByDescending(t => t.CreatedOn)
                : query.OrderBy(t => t.CreatedOn),
            "completion" or "status" => sort.IsDescending
                ? query.OrderByDescending(t => t.Status)
                : query.OrderBy(t => t.Status),
            _ => sort.IsDescending
                ? query.OrderByDescending(t => t.Deadline)
                : query.OrderBy(t => t.Deadline)
        };
    }
}
