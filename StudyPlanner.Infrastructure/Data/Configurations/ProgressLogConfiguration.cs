using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyPlanner.Core.Entities;

namespace StudyPlanner.Infrastructure.Data.Configurations;

public class ProgressLogConfiguration : IEntityTypeConfiguration<ProgressLog>
{
    public void Configure(EntityTypeBuilder<ProgressLog> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.HoursStudied)
            .HasPrecision(10, 2);

        builder.Property(p => p.Notes)
            .HasMaxLength(1000);

        builder.HasOne(p => p.User)
            .WithMany(u => u.ProgressLogs)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Subject)
            .WithMany(s => s.ProgressLogs)
            .HasForeignKey(p => p.SubjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => new { p.UserId, p.StudyDate });
    }
}
