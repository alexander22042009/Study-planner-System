using Microsoft.EntityFrameworkCore;
using StudyPlanner.Core.Entities;
using StudyPlanner.Core.Interfaces.Repositories;
using StudyPlanner.Core.Models.Common;
using StudyPlanner.Infrastructure.Data;

namespace StudyPlanner.Infrastructure.Repositories;

public class ProgressRepository : GenericRepository<ProgressLog>, IProgressRepository
{
    public ProgressRepository(ApplicationDbContext context)
        : base(context)
    {
    }

    public async Task<ProgressLog?> GetByIdForUserAsync(int id, string userId, CancellationToken cancellationToken = default) =>
        await DbSet
            .AsNoTracking()
            .Include(p => p.Subject)
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId, cancellationToken);

    public async Task<PagedResult<ProgressLog>> GetPagedForUserAsync(
        string userId,
        PaginationQuery pagination,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .AsNoTracking()
            .Include(p => p.Subject)
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.StudyDate);

        return await query.ToPagedResultAsync(pagination, cancellationToken);
    }

    public async Task<IReadOnlyList<ProgressLog>> GetWeeklyForUserAsync(
        string userId,
        DateTime weekStart,
        DateTime weekEnd,
        CancellationToken cancellationToken = default) =>
        await DbSet
            .AsNoTracking()
            .Where(p => p.UserId == userId && p.StudyDate >= weekStart && p.StudyDate < weekEnd)
            .OrderBy(p => p.StudyDate)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ProgressLog>> GetMonthlyForUserAsync(
        string userId,
        int year,
        int month,
        CancellationToken cancellationToken = default)
    {
        var start = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var end = start.AddMonths(1);

        return await DbSet
            .AsNoTracking()
            .Where(p => p.UserId == userId && p.StudyDate >= start && p.StudyDate < end)
            .OrderBy(p => p.StudyDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<decimal> GetTotalHoursForUserAsync(string userId, CancellationToken cancellationToken = default) =>
        await DbSet
            .Where(p => p.UserId == userId)
            .SumAsync(p => p.HoursStudied, cancellationToken);

    public Task<bool> ExistsForUserAsync(int id, string userId, CancellationToken cancellationToken = default) =>
        DbSet.AnyAsync(p => p.Id == id && p.UserId == userId, cancellationToken);
}
