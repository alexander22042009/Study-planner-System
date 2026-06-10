using AutoMapper;
using Microsoft.EntityFrameworkCore;
using StudyPlanner.Core.Constants;
using StudyPlanner.Core.DTOs.Achievements;
using StudyPlanner.Core.Entities;
using StudyPlanner.Core.Exceptions;
using StudyPlanner.Core.Interfaces.Repositories;
using StudyPlanner.Core.Interfaces.Services;
using StudyPlanner.Core.Models.Common;
using StudyPlanner.Infrastructure.Data;

namespace StudyPlanner.Infrastructure.Services;

public class AchievementService : IAchievementService
{
    private readonly IAchievementRepository _achievementRepository;
    private readonly ISessionRepository _sessionRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly IProgressRepository _progressRepository;
    private readonly IGoalRepository _goalRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ApplicationDbContext _context;

    public AchievementService(
        IAchievementRepository achievementRepository,
        ISessionRepository sessionRepository,
        ITaskRepository taskRepository,
        IProgressRepository progressRepository,
        IGoalRepository goalRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ApplicationDbContext context)
    {
        _achievementRepository = achievementRepository;
        _sessionRepository = sessionRepository;
        _taskRepository = taskRepository;
        _progressRepository = progressRepository;
        _goalRepository = goalRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _context = context;
    }

    public async Task<PagedResult<AchievementListDto>> GetAllAsync(
        string userId,
        PaginationQuery pagination,
        CancellationToken cancellationToken = default)
    {
        var result = await _achievementRepository.GetPagedForUserAsync(userId, pagination, cancellationToken);

        return PagedResult<AchievementListDto>.Create(
            _mapper.Map<IReadOnlyList<AchievementListDto>>(result.Items),
            result.PageNumber,
            result.PageSize,
            result.TotalCount);
    }

    public async Task<AchievementDetailsDto> GetByIdAsync(int id, string userId, CancellationToken cancellationToken = default)
    {
        var achievement = await _achievementRepository.GetByIdForUserAsync(id, userId, cancellationToken);
        if (achievement is null)
        {
            throw new NotFoundException($"Achievement with id {id} was not found.");
        }

        return _mapper.Map<AchievementDetailsDto>(achievement);
    }

    public async Task<AchievementDetailsDto> CreateAsync(CreateAchievementDto dto, string userId, CancellationToken cancellationToken = default)
    {
        var existing = await _achievementRepository.GetByTitleForUserAsync(dto.Title, userId, cancellationToken);
        if (existing is not null)
        {
            throw new BadRequestException("An achievement with this title already exists.");
        }

        var achievement = _mapper.Map<Achievement>(dto);
        achievement.UserId = userId;

        await _achievementRepository.AddAsync(achievement, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<AchievementDetailsDto>(achievement);
    }

    public async Task<AchievementDetailsDto> UpdateAsync(int id, EditAchievementDto dto, string userId, CancellationToken cancellationToken = default)
    {
        if (!await _achievementRepository.ExistsForUserAsync(id, userId, cancellationToken))
        {
            throw new NotFoundException($"Achievement with id {id} was not found.");
        }

        var achievement = await _achievementRepository.GetByIdAsync(id, cancellationToken);
        if (achievement is null)
        {
            throw new NotFoundException($"Achievement with id {id} was not found.");
        }

        _mapper.Map(dto, achievement);
        _achievementRepository.Update(achievement);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<AchievementDetailsDto>(achievement);
    }

    public async Task DeleteAsync(int id, string userId, CancellationToken cancellationToken = default)
    {
        if (!await _achievementRepository.ExistsForUserAsync(id, userId, cancellationToken))
        {
            throw new NotFoundException($"Achievement with id {id} was not found.");
        }

        var achievement = await _achievementRepository.GetByIdAsync(id, cancellationToken);
        if (achievement is not null)
        {
            _achievementRepository.Remove(achievement);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<IReadOnlyList<AchievementListDto>> GetUserAchievementsAsync(string userId, CancellationToken cancellationToken = default)
    {
        var achievements = await _achievementRepository.GetAllForUserAsync(userId, cancellationToken);
        return _mapper.Map<IReadOnlyList<AchievementListDto>>(achievements);
    }

    public async Task<AchievementDetailsDto> UnlockAchievementAsync(int id, string userId, CancellationToken cancellationToken = default)
    {
        var achievement = await _achievementRepository.GetByIdAsync(id, cancellationToken);
        if (achievement is null || achievement.UserId != userId)
        {
            throw new NotFoundException($"Achievement with id {id} was not found.");
        }

        if (achievement.UnlockedDate is null)
        {
            achievement.UnlockedDate = DateTime.UtcNow;
            _achievementRepository.Update(achievement);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return _mapper.Map<AchievementDetailsDto>(achievement);
    }

    public async Task EvaluateAchievementsAsync(string userId, CancellationToken cancellationToken = default)
    {
        var sessionCount = await _sessionRepository.GetCountForUserAsync(userId, cancellationToken);
        var completedTasks = await _taskRepository.GetCompletedCountForUserAsync(userId, cancellationToken);
        var totalHours = await _progressRepository.GetTotalHoursForUserAsync(userId, cancellationToken);
        var completedGoals = await _goalRepository.GetCompletedCountForUserAsync(userId, cancellationToken);
        var hasStreak = await HasThirtyDayStreakAsync(userId, cancellationToken);

        await TryUnlockAsync(userId, AchievementTitles.FirstStudySession, "Complete your first study session.", 10, sessionCount >= 1, cancellationToken);
        await TryUnlockAsync(userId, AchievementTitles.TenCompletedTasks, "Complete 10 study tasks.", 50, completedTasks >= 10, cancellationToken);
        await TryUnlockAsync(userId, AchievementTitles.FiftyStudyHours, "Study for 50 hours total.", 75, totalHours >= 50, cancellationToken);
        await TryUnlockAsync(userId, AchievementTitles.FirstGoalCompleted, "Complete your first learning goal.", 25, completedGoals >= 1, cancellationToken);
        await TryUnlockAsync(userId, AchievementTitles.HundredStudyHours, "Study for 100 hours total.", 150, totalHours >= 100, cancellationToken);
        await TryUnlockAsync(userId, AchievementTitles.ThirtyDayStreak, "Study for 30 consecutive days.", 200, hasStreak, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task TryUnlockAsync(
        string userId,
        string title,
        string description,
        int points,
        bool condition,
        CancellationToken cancellationToken)
    {
        if (!condition)
        {
            return;
        }

        var achievement = await _achievementRepository.GetByTitleForUserAsync(title, userId, cancellationToken);
        if (achievement is null)
        {
            achievement = new Achievement
            {
                UserId = userId,
                Title = title,
                Description = description,
                Points = points
            };
            await _achievementRepository.AddAsync(achievement, cancellationToken);
        }

        if (achievement.UnlockedDate is null)
        {
            achievement.UnlockedDate = DateTime.UtcNow;
            _achievementRepository.Update(achievement);
        }
    }

    private async Task<bool> HasThirtyDayStreakAsync(string userId, CancellationToken cancellationToken)
    {
        var studyDates = await _context.ProgressLogs
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .Select(p => p.StudyDate.Date)
            .Distinct()
            .OrderByDescending(d => d)
            .ToListAsync(cancellationToken);

        if (studyDates.Count < 30)
        {
            return false;
        }

        var streak = 1;
        for (var i = 1; i < studyDates.Count; i++)
        {
            if (studyDates[i - 1].AddDays(-1) == studyDates[i])
            {
                streak++;
                if (streak >= 30)
                {
                    return true;
                }
            }
            else
            {
                streak = 1;
            }
        }

        return false;
    }
}
