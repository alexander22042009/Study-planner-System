using Microsoft.EntityFrameworkCore;
using StudyPlanner.Core.Entities;
using StudyPlanner.Core.Interfaces.Repositories;
using StudyPlanner.Core.Models.Common;
using StudyPlanner.Infrastructure.Data;

namespace StudyPlanner.Infrastructure.Repositories;

public class SessionRepository : GenericRepository<StudySession>, ISessionRepository
{
    public SessionRepository(ApplicationDbContext context)
        : base(context)
    {
    }

    public async Task<StudySession?> GetByIdForUserAsync(int id, string userId, CancellationToken cancellationToken = default) =>
        await DbSet
            .AsNoTracking()
            .Include(s => s.Subject)
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId, cancellationToken);

    public async Task<PagedResult<StudySession>> GetPagedForUserAsync(
        string userId,
        PaginationQuery pagination,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .AsNoTracking()
            .Include(s => s.Subject)
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.StartTime);

        return await query.ToPagedResultAsync(pagination, cancellationToken);
    }

    public async Task<IReadOnlyList<StudySession>> GetWeeklyForUserAsync(
        string userId,
        DateTime weekStart,
        DateTime weekEnd,
        CancellationToken cancellationToken = default) =>
        await DbSet
            .AsNoTracking()
            .Include(s => s.Subject)
            .Where(s => s.UserId == userId && s.StartTime >= weekStart && s.StartTime < weekEnd)
            .OrderBy(s => s.StartTime)
            .ToListAsync(cancellationToken);

    public Task<bool> ExistsForUserAsync(int id, string userId, CancellationToken cancellationToken = default) =>
        DbSet.AnyAsync(s => s.Id == id && s.UserId == userId, cancellationToken);

    public Task<int> GetCountForUserAsync(string userId, CancellationToken cancellationToken = default) =>
        DbSet.CountAsync(s => s.UserId == userId, cancellationToken);
}
