using StudyPlanner.Core.DTOs.Achievements;
using StudyPlanner.Core.Models.Common;

namespace StudyPlanner.Core.Interfaces.Services;

public interface IAchievementService
{
    Task<PagedResult<AchievementListDto>> GetAllAsync(
        string userId,
        PaginationQuery pagination,
        CancellationToken cancellationToken = default);

    Task<AchievementDetailsDto> GetByIdAsync(int id, string userId, CancellationToken cancellationToken = default);

    Task<AchievementDetailsDto> CreateAsync(CreateAchievementDto dto, string userId, CancellationToken cancellationToken = default);

    Task<AchievementDetailsDto> UpdateAsync(int id, EditAchievementDto dto, string userId, CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, string userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AchievementListDto>> GetUserAchievementsAsync(string userId, CancellationToken cancellationToken = default);

    Task<AchievementDetailsDto> UnlockAchievementAsync(int id, string userId, CancellationToken cancellationToken = default);

    Task EvaluateAchievementsAsync(string userId, CancellationToken cancellationToken = default);
}
