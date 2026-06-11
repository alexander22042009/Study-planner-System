using AutoMapper;
using StudyPlanner.Core.DTOs.Goals;
using StudyPlanner.Core.Entities;
using StudyPlanner.Core.Enums;
using StudyPlanner.Core.Exceptions;
using StudyPlanner.Core.Interfaces.Repositories;
using StudyPlanner.Core.Interfaces.Services;
using StudyPlanner.Core.Models.Common;

namespace StudyPlanner.Infrastructure.Services;

public class GoalService : IGoalService
{
    private readonly IGoalRepository _goalRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IAchievementService _achievementService;

    public GoalService(
        IGoalRepository goalRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IAchievementService achievementService)
    {
        _goalRepository = goalRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _achievementService = achievementService;
    }

    public async Task<PagedResult<GoalListDto>> GetAllAsync(
        string userId,
        SearchQuery search,
        SortQuery sort,
        PaginationQuery pagination,
        CancellationToken cancellationToken = default)
    {
        await MarkExpiredGoalsAsync(userId, cancellationToken);

        var result = await _goalRepository.GetPagedForUserAsync(userId, search, sort, pagination, cancellationToken);

        return PagedResult<GoalListDto>.Create(
            _mapper.Map<IReadOnlyList<GoalListDto>>(result.Items),
            result.PageNumber,
            result.PageSize,
            result.TotalCount);
    }

    public async Task<GoalDetailsDto> GetByIdAsync(int id, string userId, CancellationToken cancellationToken = default)
    {
        await MarkExpiredGoalsAsync(userId, cancellationToken);

        var goal = await _goalRepository.GetByIdForUserAsync(id, userId, cancellationToken);
        if (goal is null)
        {
            throw new NotFoundException($"Goal with id {id} was not found.");
        }

        return _mapper.Map<GoalDetailsDto>(goal);
    }

    public async Task<GoalDetailsDto> CreateAsync(CreateGoalDto dto, string userId, CancellationToken cancellationToken = default)
    {
        if (dto.Deadline <= DateTime.UtcNow)
        {
            throw new BadRequestException("Deadline must be in the future.");
        }

        var goal = _mapper.Map<Goal>(dto);
        goal.UserId = userId;
        goal.Status = GoalStatus.NotStarted;

        await _goalRepository.AddAsync(goal, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<GoalDetailsDto>(goal);
    }

    public async Task<GoalDetailsDto> UpdateAsync(int id, EditGoalDto dto, string userId, CancellationToken cancellationToken = default)
    {
        if (!await _goalRepository.ExistsForUserAsync(id, userId, cancellationToken))
        {
            throw new NotFoundException($"Goal with id {id} was not found.");
        }

        var goal = await _goalRepository.GetByIdAsync(id, cancellationToken);
        if (goal is null)
        {
            throw new NotFoundException($"Goal with id {id} was not found.");
        }

        _mapper.Map(dto, goal);
        ApplyGoalStatus(goal);

        _goalRepository.Update(goal);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<GoalDetailsDto>(goal);
    }

    public async Task DeleteAsync(int id, string userId, CancellationToken cancellationToken = default)
    {
        if (!await _goalRepository.ExistsForUserAsync(id, userId, cancellationToken))
        {
            throw new NotFoundException($"Goal with id {id} was not found.");
        }

        var goal = await _goalRepository.GetByIdAsync(id, cancellationToken);
        if (goal is not null)
        {
            _goalRepository.Remove(goal);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<GoalDetailsDto> UpdateProgressAsync(int id, UpdateGoalProgressDto dto, string userId, CancellationToken cancellationToken = default)
    {
        var goal = await _goalRepository.GetByIdAsync(id, cancellationToken);
        if (goal is null || goal.UserId != userId)
        {
            throw new NotFoundException($"Goal with id {id} was not found.");
        }

        if (dto.CurrentHours > goal.TargetHours)
        {
            throw new BadRequestException("Current hours cannot exceed target hours.");
        }

        goal.CurrentHours = dto.CurrentHours;
        ApplyGoalStatus(goal);

        _goalRepository.Update(goal);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _achievementService.EvaluateAchievementsAsync(userId, cancellationToken);

        return _mapper.Map<GoalDetailsDto>(goal);
    }

    public async Task<GoalDetailsDto> AddHoursAsync(int id, AddGoalHoursDto dto, string userId, CancellationToken cancellationToken = default)
    {
        var goal = await _goalRepository.GetByIdAsync(id, cancellationToken);
        if (goal is null || goal.UserId != userId)
        {
            throw new NotFoundException($"Goal with id {id} was not found.");
        }

        if (goal.Status == GoalStatus.Completed)
        {
            throw new BadRequestException("Goal is already completed.");
        }

        goal.CurrentHours = Math.Min(goal.CurrentHours + dto.HoursStudied, goal.TargetHours);
        ApplyGoalStatus(goal);

        _goalRepository.Update(goal);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _achievementService.EvaluateAchievementsAsync(userId, cancellationToken);

        return _mapper.Map<GoalDetailsDto>(goal);
    }

    public async Task<GoalDetailsDto> CompleteAsync(int id, string userId, CancellationToken cancellationToken = default)
    {
        var goal = await _goalRepository.GetByIdAsync(id, cancellationToken);
        if (goal is null || goal.UserId != userId)
        {
            throw new NotFoundException($"Goal with id {id} was not found.");
        }

        goal.Status = GoalStatus.Completed;
        goal.CurrentHours = goal.TargetHours;

        _goalRepository.Update(goal);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _achievementService.EvaluateAchievementsAsync(userId, cancellationToken);

        return _mapper.Map<GoalDetailsDto>(goal);
    }

    private static void ApplyGoalStatus(Goal goal)
    {
        if (goal.Status == GoalStatus.Completed)
        {
            return;
        }

        if (goal.Deadline < DateTime.UtcNow && goal.Status != GoalStatus.Completed)
        {
            goal.Status = GoalStatus.Expired;
            return;
        }

        if (goal.CurrentHours >= goal.TargetHours)
        {
            goal.Status = GoalStatus.Completed;
        }
        else if (goal.CurrentHours > 0)
        {
            goal.Status = GoalStatus.InProgress;
        }
        else
        {
            goal.Status = GoalStatus.NotStarted;
        }
    }

    private async Task MarkExpiredGoalsAsync(string userId, CancellationToken cancellationToken)
    {
        var paged = await _goalRepository.GetPagedForUserAsync(
            userId,
            new SearchQuery(),
            new SortQuery(),
            new PaginationQuery { PageNumber = 1, PageSize = PaginationQuery.MaxPageSize },
            cancellationToken);

        var now = DateTime.UtcNow;
        var expiredGoals = paged.Items
            .Where(g => g.Deadline < now && g.Status is not GoalStatus.Completed and not GoalStatus.Expired)
            .ToList();

        foreach (var goal in expiredGoals)
        {
            var tracked = await _goalRepository.GetByIdAsync(goal.Id, cancellationToken);
            if (tracked is not null)
            {
                tracked.Status = GoalStatus.Expired;
                _goalRepository.Update(tracked);
            }
        }

        if (expiredGoals.Count > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
