using AutoMapper;
using StudyPlanner.Core.DTOs.Tasks;
using StudyPlanner.Core.Entities;
using StudyPlanner.Core.Exceptions;
using StudyPlanner.Core.Interfaces.Repositories;
using StudyPlanner.Core.Interfaces.Services;
using StudyPlanner.Core.Models.Common;
using TaskStatus = StudyPlanner.Core.Enums.TaskStatus;

namespace StudyPlanner.Infrastructure.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly ISubjectRepository _subjectRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IAchievementService _achievementService;

    public TaskService(
        ITaskRepository taskRepository,
        ISubjectRepository subjectRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IAchievementService achievementService)
    {
        _taskRepository = taskRepository;
        _subjectRepository = subjectRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _achievementService = achievementService;
    }

    public async Task<PagedResult<TaskListDto>> GetAllAsync(
        string userId,
        SearchQuery search,
        SortQuery sort,
        PaginationQuery pagination,
        CancellationToken cancellationToken = default)
    {
        await MarkOverdueTasksAsync(userId, cancellationToken);

        var result = await _taskRepository.GetPagedForUserAsync(userId, search, sort, pagination, cancellationToken);

        return PagedResult<TaskListDto>.Create(
            _mapper.Map<IReadOnlyList<TaskListDto>>(result.Items),
            result.PageNumber,
            result.PageSize,
            result.TotalCount);
    }

    public async Task<TaskDetailsDto> GetByIdAsync(int id, string userId, CancellationToken cancellationToken = default)
    {
        await MarkOverdueTasksAsync(userId, cancellationToken);

        var task = await _taskRepository.GetByIdForUserAsync(id, userId, cancellationToken);
        if (task is null)
        {
            throw new NotFoundException($"Task with id {id} was not found.");
        }

        return _mapper.Map<TaskDetailsDto>(task);
    }

    public async Task<TaskDetailsDto> CreateAsync(CreateTaskDto dto, string userId, CancellationToken cancellationToken = default)
    {
        if (dto.Deadline <= DateTime.UtcNow)
        {
            throw new BadRequestException("Deadline must be in the future.");
        }

        await EnsureSubjectBelongsToUserAsync(dto.SubjectId, userId, cancellationToken);

        var task = _mapper.Map<StudyTask>(dto);
        task.UserId = userId;
        task.Status = TaskStatus.Pending;
        task.CreatedOn = DateTime.UtcNow;

        await _taskRepository.AddAsync(task, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var created = await _taskRepository.GetByIdForUserAsync(task.Id, userId, cancellationToken);
        return _mapper.Map<TaskDetailsDto>(created);
    }

    public async Task<TaskDetailsDto> UpdateAsync(int id, EditTaskDto dto, string userId, CancellationToken cancellationToken = default)
    {
        if (!await _taskRepository.ExistsForUserAsync(id, userId, cancellationToken))
        {
            throw new NotFoundException($"Task with id {id} was not found.");
        }

        await EnsureSubjectBelongsToUserAsync(dto.SubjectId, userId, cancellationToken);

        var task = await _taskRepository.GetByIdAsync(id, cancellationToken);
        if (task is null)
        {
            throw new NotFoundException($"Task with id {id} was not found.");
        }

        _mapper.Map(dto, task);

        if (task.Status == TaskStatus.Completed && task.CompletedOn is null)
        {
            task.CompletedOn = DateTime.UtcNow;
        }
        else if (task.Status != TaskStatus.Completed)
        {
            task.CompletedOn = null;
        }

        _taskRepository.Update(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updated = await _taskRepository.GetByIdForUserAsync(id, userId, cancellationToken);
        return _mapper.Map<TaskDetailsDto>(updated);
    }

    public async Task DeleteAsync(int id, string userId, CancellationToken cancellationToken = default)
    {
        if (!await _taskRepository.ExistsForUserAsync(id, userId, cancellationToken))
        {
            throw new NotFoundException($"Task with id {id} was not found.");
        }

        var task = await _taskRepository.GetByIdAsync(id, cancellationToken);
        if (task is not null)
        {
            _taskRepository.Remove(task);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<TaskDetailsDto> CompleteAsync(int id, string userId, CancellationToken cancellationToken = default)
    {
        var task = await _taskRepository.GetByIdAsync(id, cancellationToken);
        if (task is null || task.UserId != userId)
        {
            throw new NotFoundException($"Task with id {id} was not found.");
        }

        task.Status = TaskStatus.Completed;
        task.CompletedOn = DateTime.UtcNow;

        _taskRepository.Update(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _achievementService.EvaluateAchievementsAsync(userId, cancellationToken);

        var completed = await _taskRepository.GetByIdForUserAsync(id, userId, cancellationToken);
        return _mapper.Map<TaskDetailsDto>(completed);
    }

    public async Task<PagedResult<TaskListDto>> GetBySubjectAsync(
        int subjectId,
        string userId,
        PaginationQuery pagination,
        CancellationToken cancellationToken = default)
    {
        await EnsureSubjectBelongsToUserAsync(subjectId, userId, cancellationToken);

        var result = await _taskRepository.GetBySubjectForUserAsync(subjectId, userId, pagination, cancellationToken);

        return PagedResult<TaskListDto>.Create(
            _mapper.Map<IReadOnlyList<TaskListDto>>(result.Items),
            result.PageNumber,
            result.PageSize,
            result.TotalCount);
    }

    public async Task<IReadOnlyList<TaskListDto>> GetUpcomingAsync(
        string userId,
        int daysAhead,
        CancellationToken cancellationToken = default)
    {
        if (daysAhead < 1 || daysAhead > 365)
        {
            throw new BadRequestException("Days ahead must be between 1 and 365.");
        }

        await MarkOverdueTasksAsync(userId, cancellationToken);

        var tasks = await _taskRepository.GetUpcomingForUserAsync(userId, daysAhead, cancellationToken);
        return _mapper.Map<IReadOnlyList<TaskListDto>>(tasks);
    }

    private async Task EnsureSubjectBelongsToUserAsync(int subjectId, string userId, CancellationToken cancellationToken)
    {
        if (!await _subjectRepository.ExistsForUserAsync(subjectId, userId, cancellationToken))
        {
            throw new BadRequestException($"Subject with id {subjectId} was not found.");
        }
    }

    private async Task MarkOverdueTasksAsync(string userId, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var paged = await _taskRepository.GetPagedForUserAsync(
            userId,
            new SearchQuery(),
            new SortQuery(),
            new PaginationQuery { PageNumber = 1, PageSize = PaginationQuery.MaxPageSize },
            cancellationToken);

        var overdueTasks = paged.Items
            .Where(t => t.Deadline < now && t.Status is TaskStatus.Pending or TaskStatus.InProgress)
            .ToList();

        if (overdueTasks.Count == 0)
        {
            return;
        }

        foreach (var task in overdueTasks)
        {
            var tracked = await _taskRepository.GetByIdAsync(task.Id, cancellationToken);
            if (tracked is not null)
            {
                tracked.Status = TaskStatus.Overdue;
                _taskRepository.Update(tracked);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
