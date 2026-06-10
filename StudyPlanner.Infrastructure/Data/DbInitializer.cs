using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StudyPlanner.Core.Constants;
using StudyPlanner.Core.Entities;
using StudyPlanner.Core.Enums;
using TaskStatus = StudyPlanner.Core.Enums.TaskStatus;

namespace StudyPlanner.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;

        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("DbInitializer");

        await context.Database.MigrateAsync();

        foreach (var role in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        if (await context.Subjects.AnyAsync())
        {
            logger.LogInformation("Database already seeded.");
            return;
        }

        var admin = await CreateUserAsync(userManager, "Admin", "User", SeedData.AdminEmail, SeedData.AdminPassword, Roles.Administrator);
        var student1 = await CreateUserAsync(userManager, "Alex", "Johnson", SeedData.Student1Email, SeedData.StudentPassword, Roles.Student);
        var student2 = await CreateUserAsync(userManager, "Maria", "Petrova", SeedData.Student2Email, SeedData.StudentPassword, Roles.Student);
        var student3 = await CreateUserAsync(userManager, "Ivan", "Georgiev", SeedData.Student3Email, SeedData.StudentPassword, Roles.Student);

        var subjects = new List<Subject>
        {
            new() { Name = "Mathematics", Description = "Algebra and calculus", Color = "#e74c3c", UserId = student1.Id, CreatedOn = DateTime.UtcNow.AddDays(-30) },
            new() { Name = "Programming", Description = "C# and ASP.NET Core", Color = "#3498db", UserId = student1.Id, CreatedOn = DateTime.UtcNow.AddDays(-28) },
            new() { Name = "Database Systems", Description = "SQL Server and EF Core", Color = "#9b59b6", UserId = student2.Id, CreatedOn = DateTime.UtcNow.AddDays(-25) },
            new() { Name = "English", Description = "Technical writing", Color = "#2ecc71", UserId = student2.Id, CreatedOn = DateTime.UtcNow.AddDays(-20) },
            new() { Name = "Physics", Description = "Mechanics and thermodynamics", Color = "#f39c12", UserId = student3.Id, CreatedOn = DateTime.UtcNow.AddDays(-15) }
        };
        context.Subjects.AddRange(subjects);
        await context.SaveChangesAsync();

        var tasks = new List<StudyTask>();
        var taskTitles = new[]
        {
            "Complete homework set 1", "Read chapter 3", "Solve practice problems",
            "Prepare presentation", "Review lecture notes", "Finish lab report",
            "Study for quiz", "Complete assignment 2", "Watch tutorial videos",
            "Write summary essay", "Practice coding exercises", "Group project work",
            "Prepare exam notes", "Complete online test", "Debug application",
            "Design database schema", "Implement API endpoint", "Write unit tests",
            "Create documentation", "Final project submission"
        };

        var students = new[] { student1, student2, student3 };
        for (var i = 0; i < taskTitles.Length; i++)
        {
            var student = students[i % students.Length];
            var subject = subjects.First(s => s.UserId == student.Id);

            tasks.Add(new StudyTask
            {
                Title = taskTitles[i],
                Description = $"Task description for {taskTitles[i]}",
                Deadline = DateTime.UtcNow.AddDays(i % 14 + 1),
                Priority = (TaskPriority)(i % 4),
                Status = i % 5 == 0 ? TaskStatus.Completed : i % 7 == 0 ? TaskStatus.InProgress : TaskStatus.Pending,
                CreatedOn = DateTime.UtcNow.AddDays(-(20 - i)),
                CompletedOn = i % 5 == 0 ? DateTime.UtcNow.AddDays(-(10 - i % 10)) : null,
                SubjectId = subject.Id,
                UserId = student.Id
            });
        }
        context.StudyTasks.AddRange(tasks);
        await context.SaveChangesAsync();

        var sessions = new List<StudySession>();
        for (var i = 0; i < 10; i++)
        {
            var student = students[i % students.Length];
            var subject = subjects.First(s => s.UserId == student.Id);
            var start = DateTime.UtcNow.AddDays(-i).Date.AddHours(10 + i);
            var end = start.AddMinutes(60 + i * 5);

            sessions.Add(new StudySession
            {
                Title = $"Study session {i + 1}",
                StartTime = start,
                EndTime = end,
                Duration = (int)(end - start).TotalMinutes,
                Notes = $"Focused study on {subject.Name}",
                SubjectId = subject.Id,
                UserId = student.Id
            });
        }
        context.StudySessions.AddRange(sessions);
        await context.SaveChangesAsync();

        var goals = new List<Goal>
        {
            new() { Title = "Master Algebra", Description = "Complete all algebra topics", TargetHours = 40, CurrentHours = 15, Deadline = DateTime.UtcNow.AddDays(30), Status = GoalStatus.InProgress, UserId = student1.Id },
            new() { Title = "Build REST API", Description = "Finish capstone API project", TargetHours = 60, CurrentHours = 60, Deadline = DateTime.UtcNow.AddDays(14), Status = GoalStatus.Completed, UserId = student1.Id },
            new() { Title = "SQL Certification", Description = "Prepare for SQL exam", TargetHours = 50, CurrentHours = 20, Deadline = DateTime.UtcNow.AddDays(45), Status = GoalStatus.InProgress, UserId = student2.Id },
            new() { Title = "English Fluency", Description = "Improve technical English", TargetHours = 30, CurrentHours = 5, Deadline = DateTime.UtcNow.AddDays(60), Status = GoalStatus.NotStarted, UserId = student2.Id },
            new() { Title = "Physics Exam", Description = "Pass final physics exam", TargetHours = 35, CurrentHours = 10, Deadline = DateTime.UtcNow.AddDays(20), Status = GoalStatus.InProgress, UserId = student3.Id }
        };
        context.Goals.AddRange(goals);
        await context.SaveChangesAsync();

        var progressLogs = new List<ProgressLog>();
        for (var i = 0; i < 20; i++)
        {
            var student = students[i % students.Length];
            var subject = subjects.First(s => s.UserId == student.Id);

            progressLogs.Add(new ProgressLog
            {
                StudyDate = DateTime.UtcNow.AddDays(-i),
                HoursStudied = 1.5m + i % 3,
                Notes = $"Daily study log {i + 1}",
                UserId = student.Id,
                SubjectId = subject.Id
            });
        }
        context.ProgressLogs.AddRange(progressLogs);
        await context.SaveChangesAsync();

        foreach (var student in students)
        {
            var hours = await context.ProgressLogs.Where(p => p.UserId == student.Id).SumAsync(p => p.HoursStudied);
            student.TotalStudyHours = hours;
            await userManager.UpdateAsync(student);
        }

        var achievements = new List<Achievement>
        {
            new() { Title = AchievementTitles.FirstStudySession, Description = "Complete your first study session.", Points = 10, UnlockedDate = DateTime.UtcNow.AddDays(-5), UserId = student1.Id },
            new() { Title = AchievementTitles.TenCompletedTasks, Description = "Complete 10 study tasks.", Points = 50, UnlockedDate = null, UserId = student1.Id },
            new() { Title = AchievementTitles.FiftyStudyHours, Description = "Study for 50 hours total.", Points = 75, UnlockedDate = null, UserId = student2.Id },
            new() { Title = AchievementTitles.FirstGoalCompleted, Description = "Complete your first learning goal.", Points = 25, UnlockedDate = DateTime.UtcNow.AddDays(-2), UserId = student1.Id },
            new() { Title = AchievementTitles.HundredStudyHours, Description = "Study for 100 hours total.", Points = 150, UnlockedDate = null, UserId = student3.Id }
        };
        context.Achievements.AddRange(achievements);
        await context.SaveChangesAsync();

        logger.LogInformation("Database seeded successfully.");
        _ = admin;
    }

    private static async Task<ApplicationUser> CreateUserAsync(
        UserManager<ApplicationUser> userManager,
        string firstName,
        string lastName,
        string email,
        string password,
        string role)
    {
        var existing = await userManager.FindByEmailAsync(email);
        if (existing is not null)
        {
            return existing;
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            DateCreated = DateTime.UtcNow.AddDays(-60),
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        await userManager.AddToRoleAsync(user, role);
        return user;
    }
}
