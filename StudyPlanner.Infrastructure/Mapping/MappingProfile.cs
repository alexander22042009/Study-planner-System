using AutoMapper;
using StudyPlanner.Core.DTOs.Achievements;
using StudyPlanner.Core.DTOs.Admin;
using StudyPlanner.Core.DTOs.Goals;
using StudyPlanner.Core.DTOs.Progress;
using StudyPlanner.Core.DTOs.Sessions;
using StudyPlanner.Core.DTOs.Subjects;
using StudyPlanner.Core.DTOs.Tasks;
using StudyPlanner.Core.Entities;

namespace StudyPlanner.Infrastructure.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Subject, SubjectListDto>();
        CreateMap<Subject, SubjectDetailsDto>()
            .ForMember(d => d.TaskCount, o => o.MapFrom(s => s.StudyTasks.Count))
            .ForMember(d => d.SessionCount, o => o.MapFrom(s => s.StudySessions.Count));
        CreateMap<CreateSubjectDto, Subject>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.UserId, o => o.Ignore())
            .ForMember(d => d.CreatedOn, o => o.Ignore())
            .ForMember(d => d.User, o => o.Ignore())
            .ForMember(d => d.StudyTasks, o => o.Ignore())
            .ForMember(d => d.StudySessions, o => o.Ignore())
            .ForMember(d => d.ProgressLogs, o => o.Ignore());
        CreateMap<EditSubjectDto, Subject>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.UserId, o => o.Ignore())
            .ForMember(d => d.CreatedOn, o => o.Ignore())
            .ForMember(d => d.User, o => o.Ignore())
            .ForMember(d => d.StudyTasks, o => o.Ignore())
            .ForMember(d => d.StudySessions, o => o.Ignore())
            .ForMember(d => d.ProgressLogs, o => o.Ignore());

        CreateMap<StudyTask, TaskListDto>()
            .ForMember(d => d.SubjectName, o => o.MapFrom(s => s.Subject != null ? s.Subject.Name : string.Empty));
        CreateMap<StudyTask, TaskDetailsDto>()
            .ForMember(d => d.SubjectName, o => o.MapFrom(s => s.Subject != null ? s.Subject.Name : string.Empty));
        CreateMap<CreateTaskDto, StudyTask>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.UserId, o => o.Ignore())
            .ForMember(d => d.Status, o => o.Ignore())
            .ForMember(d => d.CreatedOn, o => o.Ignore())
            .ForMember(d => d.CompletedOn, o => o.Ignore())
            .ForMember(d => d.Subject, o => o.Ignore())
            .ForMember(d => d.User, o => o.Ignore());
        CreateMap<EditTaskDto, StudyTask>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.UserId, o => o.Ignore())
            .ForMember(d => d.CreatedOn, o => o.Ignore())
            .ForMember(d => d.CompletedOn, o => o.Ignore())
            .ForMember(d => d.Subject, o => o.Ignore())
            .ForMember(d => d.User, o => o.Ignore());

        CreateMap<StudySession, SessionListDto>()
            .ForMember(d => d.SubjectName, o => o.MapFrom(s => s.Subject != null ? s.Subject.Name : string.Empty));
        CreateMap<StudySession, SessionDetailsDto>()
            .ForMember(d => d.SubjectName, o => o.MapFrom(s => s.Subject != null ? s.Subject.Name : string.Empty));
        CreateMap<CreateSessionDto, StudySession>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.UserId, o => o.Ignore())
            .ForMember(d => d.Duration, o => o.Ignore())
            .ForMember(d => d.Subject, o => o.Ignore())
            .ForMember(d => d.User, o => o.Ignore());
        CreateMap<EditSessionDto, StudySession>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.UserId, o => o.Ignore())
            .ForMember(d => d.Duration, o => o.Ignore())
            .ForMember(d => d.Subject, o => o.Ignore())
            .ForMember(d => d.User, o => o.Ignore());

        CreateMap<Goal, GoalListDto>();
        CreateMap<Goal, GoalDetailsDto>()
            .ForMember(d => d.ProgressPercentage, o => o.MapFrom(g =>
                g.TargetHours > 0 ? Math.Round(g.CurrentHours / g.TargetHours * 100, 2) : 0));
        CreateMap<CreateGoalDto, Goal>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.UserId, o => o.Ignore())
            .ForMember(d => d.CurrentHours, o => o.Ignore())
            .ForMember(d => d.Status, o => o.Ignore())
            .ForMember(d => d.User, o => o.Ignore());
        CreateMap<EditGoalDto, Goal>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.UserId, o => o.Ignore())
            .ForMember(d => d.CurrentHours, o => o.Ignore())
            .ForMember(d => d.User, o => o.Ignore());

        CreateMap<ProgressLog, ProgressListDto>()
            .ForMember(d => d.SubjectName, o => o.MapFrom(p => p.Subject != null ? p.Subject.Name : string.Empty));
        CreateMap<ProgressLog, ProgressDetailsDto>()
            .ForMember(d => d.SubjectName, o => o.MapFrom(p => p.Subject != null ? p.Subject.Name : string.Empty));
        CreateMap<CreateProgressDto, ProgressLog>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.UserId, o => o.Ignore())
            .ForMember(d => d.Subject, o => o.Ignore())
            .ForMember(d => d.User, o => o.Ignore());
        CreateMap<EditProgressDto, ProgressLog>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.UserId, o => o.Ignore())
            .ForMember(d => d.Subject, o => o.Ignore())
            .ForMember(d => d.User, o => o.Ignore());

        CreateMap<Achievement, AchievementListDto>()
            .ForMember(d => d.IsUnlocked, o => o.MapFrom(a => a.UnlockedDate != null));
        CreateMap<Achievement, AchievementDetailsDto>()
            .ForMember(d => d.IsUnlocked, o => o.MapFrom(a => a.UnlockedDate != null));
        CreateMap<CreateAchievementDto, Achievement>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.UserId, o => o.Ignore())
            .ForMember(d => d.UnlockedDate, o => o.Ignore())
            .ForMember(d => d.User, o => o.Ignore());
        CreateMap<EditAchievementDto, Achievement>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.UserId, o => o.Ignore())
            .ForMember(d => d.UnlockedDate, o => o.Ignore())
            .ForMember(d => d.User, o => o.Ignore());

        CreateMap<ApplicationUser, UserListDto>()
            .ForMember(d => d.Roles, o => o.Ignore());
        CreateMap<ApplicationUser, UserDetailsDto>()
            .ForMember(d => d.Roles, o => o.Ignore())
            .ForMember(d => d.SubjectCount, o => o.MapFrom(u => u.Subjects.Count))
            .ForMember(d => d.TaskCount, o => o.MapFrom(u => u.StudyTasks.Count))
            .ForMember(d => d.GoalCount, o => o.MapFrom(u => u.Goals.Count))
            .ForMember(d => d.AchievementCount, o => o.MapFrom(u => u.Achievements.Count));
    }
}
