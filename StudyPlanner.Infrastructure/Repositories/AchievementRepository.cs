using Microsoft.EntityFrameworkCore;
using StudyPlanner.Core.Entities;
using StudyPlanner.Core.Interfaces.Repositories;
using StudyPlanner.Core.Models.Common;
using StudyPlanner.Infrastructure.Data;

namespace StudyPlanner.Infrastructure.Repositories;

public class AchievementRepository : GenericRepository<Achievement>, IAchievementRepository
{
    public AchievementRepository(ApplicationDbContext context)
        : base(context)
    {
    }

    public async Task<Achievement?> GetByIdForUserAsync(int id, string userId, CancellationToken cancellationToken = default) =>
        await DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId, cancellationToken);

    public async Task<IReadOnlyList<Achievement>> GetAllForUserAsync(string userId, CancellationToken cancellationToken = default) =>
        await DbSet
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.UnlockedDate)
            .ToListAsync(cancellationToken);

    public async Task<PagedResult<Achievement>> GetPagedForUserAsync(
        string userId,
        PaginationQuery pagination,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.UnlockedDate);

        return await query.ToPagedResultAsync(pagination, cancellationToken);
    }

    public async Task<Achievement?> GetByTitleForUserAsync(string title, string userId, CancellationToken cancellationToken = default) =>
        await DbSet
            .FirstOrDefaultAsync(a => a.Title == title && a.UserId == userId, cancellationToken);

    public async Task<int> GetTotalPointsForUserAsync(string userId, CancellationToken cancellationToken = default) =>
        await DbSet
            .Where(a => a.UserId == userId && a.UnlockedDate != null)
            .SumAsync(a => a.Points, cancellationToken);

    public Task<bool> ExistsForUserAsync(int id, string userId, CancellationToken cancellationToken = default) =>
        DbSet.AnyAsync(a => a.Id == id && a.UserId == userId, cancellationToken);
}
