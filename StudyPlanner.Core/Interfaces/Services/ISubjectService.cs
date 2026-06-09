using StudyPlanner.Core.DTOs.Subjects;
using StudyPlanner.Core.Models.Common;

namespace StudyPlanner.Core.Interfaces.Services;

public interface ISubjectService
{
    Task<PagedResult<SubjectListDto>> GetAllAsync(
        string userId,
        SearchQuery search,
        SortQuery sort,
        PaginationQuery pagination,
        CancellationToken cancellationToken = default);

    Task<SubjectDetailsDto> GetByIdAsync(int id, string userId, CancellationToken cancellationToken = default);

    Task<SubjectDetailsDto> CreateAsync(CreateSubjectDto dto, string userId, CancellationToken cancellationToken = default);

    Task<SubjectDetailsDto> UpdateAsync(int id, EditSubjectDto dto, string userId, CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, string userId, CancellationToken cancellationToken = default);
}
