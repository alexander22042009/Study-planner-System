using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StudyPlanner.Core.Entities;
namespace StudyPlanner.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Subject> Subjects => Set<Subject>();

    public DbSet<StudyTask> StudyTasks => Set<StudyTask>();

    public DbSet<StudySession> StudySessions => Set<StudySession>();

    public DbSet<Goal> Goals => Set<Goal>();

    public DbSet<ProgressLog> ProgressLogs => Set<ProgressLog>();

    public DbSet<Achievement> Achievements => Set<Achievement>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
