using AutoMapper;
using Microsoft.AspNetCore.Identity;
using StudyPlanner.Core.DTOs.Progress;
using StudyPlanner.Core.Entities;
using StudyPlanner.Core.Exceptions;
using StudyPlanner.Core.Interfaces.Repositories;
using StudyPlanner.Core.Interfaces.Services;
using StudyPlanner.Core.Models.Common;

namespace StudyPlanner.Infrastructure.Services;

public class ProgressService : IProgressService
{
    private readonly IProgressRepository _progressRepository;
    private readonly ISubjectRepository _subjectRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly IGoalRepository _goalRepository;
    private readonly IAchievementRepository _achievementRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAchievementService _achievementService;

    public ProgressService(
        IProgressRepository progressRepository,
        ISubjectRepository subjectRepository,
        ITaskRepository taskRepository,
        IGoalRepository goalRepository,
        IAchievementRepository achievementRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        UserManager<ApplicationUser> userManager,
        IAchievementService achievementService)
    {
        _progressRepository = progressRepository;
        _subjectRepository = subjectRepository;
        _taskRepository = taskRepository;
        _goalRepository = goalRepository;
        _achievementRepository = achievementRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _userManager = userManager;
        _achievementService = achievementService;
    }

    public async Task<PagedResult<ProgressListDto>> GetAllAsync(
        string userId,
        PaginationQuery pagination,
        CancellationToken cancellationToken = default)
    {
        var result = await _progressRepository.GetPagedForUserAsync(userId, pagination, cancellationToken);

        return PagedResult<ProgressListDto>.Create(
            _mapper.Map<IReadOnlyList<ProgressListDto>>(result.Items),
            result.PageNumber,
            result.PageSize,
            result.TotalCount);
    }

    public async Task<ProgressDetailsDto> GetByIdAsync(int id, string userId, CancellationToken cancellationToken = default)
    {
        var progress = await _progressRepository.GetByIdForUserAsync(id, userId, cancellationToken);
        if (progress is null)
        {
            throw new NotFoundException($"Progress log with id {id} was not found.");
        }

        return _mapper.Map<ProgressDetailsDto>(progress);
    }

    public async Task<ProgressDetailsDto> CreateAsync(CreateProgressDto dto, string userId, CancellationToken cancellationToken = default)
    {
        if (dto.StudyDate > DateTime.UtcNow)
        {
            throw new BadRequestException("Study date cannot be in the future.");
        }

        await EnsureSubjectBelongsToUserAsync(dto.SubjectId, userId, cancellationToken);

        var progress = _mapper.Map<ProgressLog>(dto);
        progress.UserId = userId;

        await _progressRepository.AddAsync(progress, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await SyncUserStudyHoursAsync(userId, cancellationToken);
        await _achievementService.EvaluateAchievementsAsync(userId, cancellationToken);

        var created = await _progressRepository.GetByIdForUserAsync(progress.Id, userId, cancellationToken);
        return _mapper.Map<ProgressDetailsDto>(created);
    }

    public async Task<ProgressDetailsDto> UpdateAsync(int id, EditProgressDto dto, string userId, CancellationToken cancellationToken = default)
    {
        if (!await _progressRepository.ExistsForUserAsync(id, userId, cancellationToken))
        {
            throw new NotFoundException($"Progress log with id {id} was not found.");
        }

        if (dto.StudyDate > DateTime.UtcNow)
        {
            throw new BadRequestException("Study date cannot be in the future.");
        }

        await EnsureSubjectBelongsToUserAsync(dto.SubjectId, userId, cancellationToken);

        var progress = await _progressRepository.GetByIdAsync(id, cancellationToken);
        if (progress is null)
        {
            throw new NotFoundException($"Progress log with id {id} was not found.");
        }

        _mapper.Map(dto, progress);
        _progressRepository.Update(progress);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await SyncUserStudyHoursAsync(userId, cancellationToken);
        await _achievementService.EvaluateAchievementsAsync(userId, cancellationToken);

        var updated = await _progressRepository.GetByIdForUserAsync(id, userId, cancellationToken);
        return _mapper.Map<ProgressDetailsDto>(updated);
    }

    public async Task DeleteAsync(int id, string userId, CancellationToken cancellationToken = default)
    {
        if (!await _progressRepository.ExistsForUserAsync(id, userId, cancellationToken))
        {
            throw new NotFoundException($"Progress log with id {id} was not found.");
        }

        var progress = await _progressRepository.GetByIdAsync(id, cancellationToken);
        if (progress is not null)
        {
            _progressRepository.Remove(progress);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await SyncUserStudyHoursAsync(userId, cancellationToken);
            await _achievementService.EvaluateAchievementsAsync(userId, cancellationToken);
        }
    }

    public async Task<StatisticsDto> GetStatisticsAsync(string userId, CancellationToken cancellationToken = default)
    {
        var weekStart = GetWeekStart(null);
        var weekEnd = weekStart.AddDays(7);
        var now = DateTime.UtcNow;

        var weeklyLogs = await _progressRepository.GetWeeklyForUserAsync(userId, weekStart, weekEnd, cancellationToken);
        var monthlyLogs = await _progressRepository.GetMonthlyForUserAsync(userId, now.Year, now.Month, cancellationToken);

        var weeklyActivity = weeklyLogs
            .GroupBy(p => p.StudyDate.Date)
            .Select(g => new WeeklyProgressDto { Date = g.Key, HoursStudied = g.Sum(p => p.HoursStudied) })
            .OrderBy(w => w.Date)
            .ToList();

        var monthlyActivity = new List<MonthlyProgressDto>
        {
            new()
            {
                Year = now.Year,
                Month = now.Month,
                HoursStudied = monthlyLogs.Sum(p => p.HoursStudied)
            }
        };

        return new StatisticsDto
        {
            TotalSubjects = (await _subjectRepository.GetPagedForUserAsync(
                userId, new SearchQuery(), new SortQuery(),
                new PaginationQuery { PageNumber = 1, PageSize = 1 }, cancellationToken)).TotalCount,
            CompletedTasks = await _taskRepository.GetCompletedCountForUserAsync(userId, cancellationToken),
            PendingTasks = await _taskRepository.GetPendingCountForUserAsync(userId, cancellationToken),
            HoursStudied = await _progressRepository.GetTotalHoursForUserAsync(userId, cancellationToken),
            GoalsCompleted = await _goalRepository.GetCompletedCountForUserAsync(userId, cancellationToken),
            AchievementPoints = await _achievementRepository.GetTotalPointsForUserAsync(userId, cancellationToken),
            WeeklyActivity = weeklyActivity,
            MonthlyActivity = monthlyActivity
        };
    }

    public async Task<IReadOnlyList<WeeklyProgressDto>> GetWeeklyProgressAsync(
        string userId,
        DateTime? weekStart,
        CancellationToken cancellationToken = default)
    {
        var start = GetWeekStart(weekStart);
        var end = start.AddDays(7);

        var logs = await _progressRepository.GetWeeklyForUserAsync(userId, start, end, cancellationToken);

        return logs
            .GroupBy(p => p.StudyDate.Date)
            .Select(g => new WeeklyProgressDto { Date = g.Key, HoursStudied = g.Sum(p => p.HoursStudied) })
            .OrderBy(w => w.Date)
            .ToList();
    }

    public async Task<IReadOnlyList<MonthlyProgressDto>> GetMonthlyProgressAsync(
        string userId,
        int? year,
        int? month,
        CancellationToken cancellationToken = default)
    {
        var targetYear = year ?? DateTime.UtcNow.Year;
        var targetMonth = month ?? DateTime.UtcNow.Month;

        if (targetMonth is < 1 or > 12)
        {
            throw new BadRequestException("Month must be between 1 and 12.");
        }

        var logs = await _progressRepository.GetMonthlyForUserAsync(userId, targetYear, targetMonth, cancellationToken);

        return
        [
            new MonthlyProgressDto
            {
                Year = targetYear,
                Month = targetMonth,
                HoursStudied = logs.Sum(p => p.HoursStudied)
            }
        ];
    }

    private async Task EnsureSubjectBelongsToUserAsync(int subjectId, string userId, CancellationToken cancellationToken)
    {
        if (!await _subjectRepository.ExistsForUserAsync(subjectId, userId, cancellationToken))
        {
            throw new BadRequestException($"Subject with id {subjectId} was not found.");
        }
    }

    private async Task SyncUserStudyHoursAsync(string userId, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return;
        }

        user.TotalStudyHours = await _progressRepository.GetTotalHoursForUserAsync(userId, cancellationToken);
        await _userManager.UpdateAsync(user);
    }

    private static DateTime GetWeekStart(DateTime? weekStart)
    {
        var date = (weekStart ?? DateTime.UtcNow).Date;
        var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-diff);
    }
}
