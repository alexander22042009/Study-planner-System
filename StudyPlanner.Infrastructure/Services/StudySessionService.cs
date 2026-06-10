using AutoMapper;
using StudyPlanner.Core.DTOs.Sessions;
using StudyPlanner.Core.Entities;
using StudyPlanner.Core.Exceptions;
using StudyPlanner.Core.Interfaces.Repositories;
using StudyPlanner.Core.Interfaces.Services;
using StudyPlanner.Core.Models.Common;

namespace StudyPlanner.Infrastructure.Services;

public class StudySessionService : IStudySessionService
{
    private readonly ISessionRepository _sessionRepository;
    private readonly ISubjectRepository _subjectRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IAchievementService _achievementService;

    public StudySessionService(
        ISessionRepository sessionRepository,
        ISubjectRepository subjectRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IAchievementService achievementService)
    {
        _sessionRepository = sessionRepository;
        _subjectRepository = subjectRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _achievementService = achievementService;
    }

    public async Task<PagedResult<SessionListDto>> GetAllAsync(
        string userId,
        PaginationQuery pagination,
        CancellationToken cancellationToken = default)
    {
        var result = await _sessionRepository.GetPagedForUserAsync(userId, pagination, cancellationToken);

        return PagedResult<SessionListDto>.Create(
            _mapper.Map<IReadOnlyList<SessionListDto>>(result.Items),
            result.PageNumber,
            result.PageSize,
            result.TotalCount);
    }

    public async Task<SessionDetailsDto> GetByIdAsync(int id, string userId, CancellationToken cancellationToken = default)
    {
        var session = await _sessionRepository.GetByIdForUserAsync(id, userId, cancellationToken);
        if (session is null)
        {
            throw new NotFoundException($"Study session with id {id} was not found.");
        }

        return _mapper.Map<SessionDetailsDto>(session);
    }

    public async Task<SessionDetailsDto> CreateAsync(CreateSessionDto dto, string userId, CancellationToken cancellationToken = default)
    {
        ValidateSessionTimes(dto.StartTime, dto.EndTime);
        await EnsureSubjectBelongsToUserAsync(dto.SubjectId, userId, cancellationToken);

        var session = _mapper.Map<StudySession>(dto);
        session.UserId = userId;
        session.Duration = CalculateDurationMinutes(dto.StartTime, dto.EndTime);

        await _sessionRepository.AddAsync(session, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _achievementService.EvaluateAchievementsAsync(userId, cancellationToken);

        var created = await _sessionRepository.GetByIdForUserAsync(session.Id, userId, cancellationToken);
        return _mapper.Map<SessionDetailsDto>(created);
    }

    public async Task<SessionDetailsDto> UpdateAsync(int id, EditSessionDto dto, string userId, CancellationToken cancellationToken = default)
    {
        if (!await _sessionRepository.ExistsForUserAsync(id, userId, cancellationToken))
        {
            throw new NotFoundException($"Study session with id {id} was not found.");
        }

        ValidateSessionTimes(dto.StartTime, dto.EndTime);
        await EnsureSubjectBelongsToUserAsync(dto.SubjectId, userId, cancellationToken);

        var session = await _sessionRepository.GetByIdAsync(id, cancellationToken);
        if (session is null)
        {
            throw new NotFoundException($"Study session with id {id} was not found.");
        }

        _mapper.Map(dto, session);
        session.Duration = CalculateDurationMinutes(dto.StartTime, dto.EndTime);

        _sessionRepository.Update(session);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updated = await _sessionRepository.GetByIdForUserAsync(id, userId, cancellationToken);
        return _mapper.Map<SessionDetailsDto>(updated);
    }

    public async Task DeleteAsync(int id, string userId, CancellationToken cancellationToken = default)
    {
        if (!await _sessionRepository.ExistsForUserAsync(id, userId, cancellationToken))
        {
            throw new NotFoundException($"Study session with id {id} was not found.");
        }

        var session = await _sessionRepository.GetByIdAsync(id, cancellationToken);
        if (session is not null)
        {
            _sessionRepository.Remove(session);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<IReadOnlyList<SessionListDto>> GetWeeklyAsync(
        string userId,
        DateTime? weekStart,
        CancellationToken cancellationToken = default)
    {
        var start = GetWeekStart(weekStart);
        var end = start.AddDays(7);

        var sessions = await _sessionRepository.GetWeeklyForUserAsync(userId, start, end, cancellationToken);
        return _mapper.Map<IReadOnlyList<SessionListDto>>(sessions);
    }

    private async Task EnsureSubjectBelongsToUserAsync(int subjectId, string userId, CancellationToken cancellationToken)
    {
        if (!await _subjectRepository.ExistsForUserAsync(subjectId, userId, cancellationToken))
        {
            throw new BadRequestException($"Subject with id {subjectId} was not found.");
        }
    }

    private static void ValidateSessionTimes(DateTime startTime, DateTime endTime)
    {
        if (endTime <= startTime)
        {
            throw new BadRequestException("End time must be after start time.");
        }
    }

    private static int CalculateDurationMinutes(DateTime startTime, DateTime endTime) =>
        (int)Math.Round((endTime - startTime).TotalMinutes);

    private static DateTime GetWeekStart(DateTime? weekStart)
    {
        var date = (weekStart ?? DateTime.UtcNow).Date;
        var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-diff);
    }
}
