using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Norm.Database.Entities;

namespace Norm.Database.EntityTypeConfigurations
{
    internal class GuildBackgroundJobETC : IEntityTypeConfiguration<GuildBackgroundJob>
    {
        public void Configure(EntityTypeBuilder<GuildBackgroundJob> builder)
        {
            builder.ToTable("all_guild_background_jobs");

            builder.HasKey(b => b.HangfireJobId)
                .HasName("hangfire_job_id");

            builder.Property(b => b.GuildId)
                .HasColumnName("guild_id")
                .IsRequired();

            builder.Property(b => b.JobName)
                .HasColumnName("job_name")
                .IsRequired();

            builder.Property(b => b.ScheduledTime)
                .HasColumnName("scheduled_time")
                .IsRequired();

            builder.Property(b => b.GuildJobType)
                .HasColumnName("job_type")
                .IsRequired();

            builder.HasIndex(b => new { b.GuildId, b.ScheduledTime });
        }
    }
}
