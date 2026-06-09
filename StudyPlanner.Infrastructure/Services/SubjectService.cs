using AutoMapper;
using StudyPlanner.Core.DTOs.Subjects;
using StudyPlanner.Core.Entities;
using StudyPlanner.Core.Exceptions;
using StudyPlanner.Core.Interfaces.Repositories;
using StudyPlanner.Core.Interfaces.Services;
using StudyPlanner.Core.Models.Common;

namespace StudyPlanner.Infrastructure.Services;

public class SubjectService : ISubjectService
{
    private readonly ISubjectRepository _subjectRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SubjectService(ISubjectRepository subjectRepository, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _subjectRepository = subjectRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResult<SubjectListDto>> GetAllAsync(
        string userId,
        SearchQuery search,
        SortQuery sort,
        PaginationQuery pagination,
        CancellationToken cancellationToken = default)
    {
        var result = await _subjectRepository.GetPagedForUserAsync(userId, search, sort, pagination, cancellationToken);

        return PagedResult<SubjectListDto>.Create(
            _mapper.Map<IReadOnlyList<SubjectListDto>>(result.Items),
            result.PageNumber,
            result.PageSize,
            result.TotalCount);
    }

    public async Task<SubjectDetailsDto> GetByIdAsync(int id, string userId, CancellationToken cancellationToken = default)
    {
        var subject = await _subjectRepository.GetByIdForUserAsync(id, userId, cancellationToken);
        if (subject is null)
        {
            throw new NotFoundException($"Subject with id {id} was not found.");
        }

        return _mapper.Map<SubjectDetailsDto>(subject);
    }

    public async Task<SubjectDetailsDto> CreateAsync(CreateSubjectDto dto, string userId, CancellationToken cancellationToken = default)
    {
        var subject = _mapper.Map<Subject>(dto);
        subject.UserId = userId;
        subject.CreatedOn = DateTime.UtcNow;

        await _subjectRepository.AddAsync(subject, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<SubjectDetailsDto>(subject);
    }

    public async Task<SubjectDetailsDto> UpdateAsync(int id, EditSubjectDto dto, string userId, CancellationToken cancellationToken = default)
    {
        var subject = await _subjectRepository.GetByIdForUserAsync(id, userId, cancellationToken);
        if (subject is null)
        {
            throw new NotFoundException($"Subject with id {id} was not found.");
        }

        var tracked = await _subjectRepository.GetByIdAsync(id, cancellationToken);
        if (tracked is null)
        {
            throw new NotFoundException($"Subject with id {id} was not found.");
        }

        _mapper.Map(dto, tracked);
        _subjectRepository.Update(tracked);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updated = await _subjectRepository.GetByIdForUserAsync(id, userId, cancellationToken);
        return _mapper.Map<SubjectDetailsDto>(updated);
    }

    public async Task DeleteAsync(int id, string userId, CancellationToken cancellationToken = default)
    {
        if (!await _subjectRepository.ExistsForUserAsync(id, userId, cancellationToken))
        {
            throw new NotFoundException($"Subject with id {id} was not found.");
        }

        var subject = await _subjectRepository.GetByIdAsync(id, cancellationToken);
        if (subject is not null)
        {
            _subjectRepository.Remove(subject);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
