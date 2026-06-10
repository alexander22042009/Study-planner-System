using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudyPlanner.Core.Constants;
using StudyPlanner.Core.DTOs.Admin;
using StudyPlanner.Core.Entities;
using StudyPlanner.Core.Enums;
using StudyPlanner.Core.Exceptions;
using StudyPlanner.Core.Interfaces.Services;
using StudyPlanner.Core.Models.Common;
using StudyPlanner.Infrastructure.Data;
using TaskStatus = StudyPlanner.Core.Enums.TaskStatus;

namespace StudyPlanner.Infrastructure.Services;

public class AdminService : IAdminService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public AdminService(UserManager<ApplicationUser> userManager, ApplicationDbContext context, IMapper mapper)
    {
        _userManager = userManager;
        _context = context;
        _mapper = mapper;
    }

    public async Task<PagedResult<UserListDto>> GetUsersAsync(PaginationQuery pagination, CancellationToken cancellationToken = default)
    {
        pagination.Normalize();

        var query = _userManager.Users.OrderByDescending(u => u.DateCreated);
        var totalCount = await query.CountAsync(cancellationToken);

        var users = await query
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync(cancellationToken);

        var items = new List<UserListDto>();
        foreach (var user in users)
        {
            var dto = _mapper.Map<UserListDto>(user);
            dto.Roles = await _userManager.GetRolesAsync(user);
            items.Add(dto);
        }

        return PagedResult<UserListDto>.Create(items, pagination.PageNumber, pagination.PageSize, totalCount);
    }

    public async Task<UserDetailsDto> GetUserByIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .AsNoTracking()
            .Include(u => u.Subjects)
            .Include(u => u.StudyTasks)
            .Include(u => u.Goals)
            .Include(u => u.Achievements)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
        {
            throw new NotFoundException($"User with id {userId} was not found.");
        }

        var dto = _mapper.Map<UserDetailsDto>(user);
        dto.Roles = await _userManager.GetRolesAsync(user);
        return dto;
    }

    public async Task DeleteUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            throw new NotFoundException($"User with id {userId} was not found.");
        }

        var isAdmin = await _userManager.IsInRoleAsync(user, Roles.Administrator);
        if (isAdmin)
        {
            var adminCount = 0;
            var admins = await _userManager.GetUsersInRoleAsync(Roles.Administrator);
            adminCount = admins.Count;

            if (adminCount <= 1)
            {
                throw new BadRequestException("Cannot delete the last administrator account.");
            }
        }

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            throw new BadRequestException(string.Join("; ", result.Errors.Select(e => e.Description)));
        }
    }

    public async Task<AdminReportDto> GetReportsAsync(CancellationToken cancellationToken = default)
    {
        var students = await _userManager.GetUsersInRoleAsync(Roles.Student);
        var administrators = await _userManager.GetUsersInRoleAsync(Roles.Administrator);

        return new AdminReportDto
        {
            TotalUsers = await _context.Users.CountAsync(cancellationToken),
            TotalStudents = students.Count,
            TotalAdministrators = administrators.Count,
            TotalSubjects = await _context.Subjects.CountAsync(cancellationToken),
            TotalTasks = await _context.StudyTasks.CountAsync(cancellationToken),
            TotalSessions = await _context.StudySessions.CountAsync(cancellationToken),
            TotalGoals = await _context.Goals.CountAsync(cancellationToken),
            TotalAchievements = await _context.Achievements.CountAsync(cancellationToken),
            TotalStudyHours = await _context.ProgressLogs.SumAsync(p => p.HoursStudied, cancellationToken),
            GeneratedAt = DateTime.UtcNow
        };
    }

    public async Task<AdminStatisticsDto> GetStatisticsAsync(CancellationToken cancellationToken = default)
    {
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
        var userCount = await _context.Users.CountAsync(cancellationToken);

        var activeUsers = await _context.ProgressLogs
            .Where(p => p.StudyDate >= thirtyDaysAgo)
            .Select(p => p.UserId)
            .Distinct()
            .CountAsync(cancellationToken);

        var totalHours = await _context.ProgressLogs.SumAsync(p => p.HoursStudied, cancellationToken);

        return new AdminStatisticsDto
        {
            ActiveUsersLast30Days = activeUsers,
            CompletedTasks = await _context.StudyTasks.CountAsync(t => t.Status == TaskStatus.Completed, cancellationToken),
            PendingTasks = await _context.StudyTasks.CountAsync(
                t => t.Status == TaskStatus.Pending || t.Status == TaskStatus.InProgress,
                cancellationToken),
            AverageStudyHoursPerUser = userCount > 0 ? Math.Round(totalHours / userCount, 2) : 0,
            GoalsCompleted = await _context.Goals.CountAsync(g => g.Status == GoalStatus.Completed, cancellationToken),
            TotalAchievementPoints = await _context.Achievements
                .Where(a => a.UnlockedDate != null)
                .SumAsync(a => a.Points, cancellationToken)
        };
    }
}
