using StudyPlanner.Core.DTOs.Progress;
using StudyPlanner.Core.Models.Common;

namespace StudyPlanner.Core.Interfaces.Services;

public interface IProgressService
{
    Task<PagedResult<ProgressListDto>> GetAllAsync(
        string userId,
        PaginationQuery pagination,
        CancellationToken cancellationToken = default);

    Task<ProgressDetailsDto> GetByIdAsync(int id, string userId, CancellationToken cancellationToken = default);

    Task<ProgressDetailsDto> CreateAsync(CreateProgressDto dto, string userId, CancellationToken cancellationToken = default);

    Task<ProgressDetailsDto> UpdateAsync(int id, EditProgressDto dto, string userId, CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, string userId, CancellationToken cancellationToken = default);

    Task<StatisticsDto> GetStatisticsAsync(string userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<WeeklyProgressDto>> GetWeeklyProgressAsync(
        string userId,
        DateTime? weekStart,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MonthlyProgressDto>> GetMonthlyProgressAsync(
        string userId,
        int? year,
        int? month,
        CancellationToken cancellationToken = default);
}
