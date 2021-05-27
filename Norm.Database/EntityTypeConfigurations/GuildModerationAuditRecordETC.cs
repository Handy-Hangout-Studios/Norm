using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Norm.Database.Entities;

namespace Norm.Database.EntityTypeConfigurations
{
    internal class GuildModerationAuditRecordETC : IEntityTypeConfiguration<GuildModerationAuditRecord>
    {
        public void Configure(EntityTypeBuilder<GuildModerationAuditRecord> builder)
        {
            builder.ToTable("all_guild_moderation_audit_records");

            builder.HasKey(ar => ar.Id)
                .HasName("audit_record_id");

            builder.Property(ar => ar.GuildId)
                .HasColumnName("guild_id")
                .IsRequired();

            builder.Property(ar => ar.ModeratorUserId)
                .HasColumnName("moderator_user_id")
                .IsRequired();

            builder.Property(ar => ar.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            builder.Property(ar => ar.ModerationAction)
                .HasColumnName("moderation_action")
                .IsRequired();

            builder.Property(ar => ar.Reason)
                .HasColumnName("reason")
                .IsRequired(false);

            builder.Property(ar => ar.Timestamp)
                .HasColumnName("timestamp")
                .IsRequired();

            builder.HasIndex(ar => ar.GuildId);
        }
    }
}
