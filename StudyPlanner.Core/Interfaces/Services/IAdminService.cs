using StudyPlanner.Core.DTOs.Admin;
using StudyPlanner.Core.Models.Common;

namespace StudyPlanner.Core.Interfaces.Services;

public interface IAdminService
{
    Task<PagedResult<UserListDto>> GetUsersAsync(PaginationQuery pagination, CancellationToken cancellationToken = default);

    Task<UserDetailsDto> GetUserByIdAsync(string userId, CancellationToken cancellationToken = default);

    Task DeleteUserAsync(string userId, CancellationToken cancellationToken = default);

    Task<AdminReportDto> GetReportsAsync(CancellationToken cancellationToken = default);

    Task<AdminStatisticsDto> GetStatisticsAsync(CancellationToken cancellationToken = default);
}
