using StudyPlanner.Core.DTOs.Sessions;
using StudyPlanner.Core.Models.Common;

namespace StudyPlanner.Core.Interfaces.Services;

public interface IStudySessionService
{
    Task<PagedResult<SessionListDto>> GetAllAsync(
        string userId,
        PaginationQuery pagination,
        CancellationToken cancellationToken = default);

    Task<SessionDetailsDto> GetByIdAsync(int id, string userId, CancellationToken cancellationToken = default);

    Task<SessionDetailsDto> CreateAsync(CreateSessionDto dto, string userId, CancellationToken cancellationToken = default);

    Task<SessionDetailsDto> UpdateAsync(int id, EditSessionDto dto, string userId, CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, string userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SessionListDto>> GetWeeklyAsync(
        string userId,
        DateTime? weekStart,
        CancellationToken cancellationToken = default);
}
