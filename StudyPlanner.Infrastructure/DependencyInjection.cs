using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StudyPlanner.Core.Interfaces.Repositories;
using StudyPlanner.Core.Interfaces.Services;
using StudyPlanner.Core.Settings;
using StudyPlanner.Infrastructure.Data;
using StudyPlanner.Infrastructure.Identity;
using StudyPlanner.Infrastructure.Mapping;
using StudyPlanner.Infrastructure.Repositories;
using StudyPlanner.Infrastructure.Services;

namespace StudyPlanner.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddAutoMapper(typeof(MappingProfile).Assembly);

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ISubjectRepository, SubjectRepository>();
        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped<IGoalRepository, GoalRepository>();
        services.AddScoped<IProgressRepository, ProgressRepository>();
        services.AddScoped<IAchievementRepository, AchievementRepository>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ISubjectService, SubjectService>();
        services.AddScoped<ITaskService, TaskService>();
        services.AddScoped<IAchievementService, AchievementService>();
        services.AddScoped<IStudySessionService, StudySessionService>();
        services.AddScoped<IGoalService, GoalService>();
        services.AddScoped<IProgressService, ProgressService>();
        services.AddScoped<IAdminService, AdminService>();

        return services;
    }
}
