using Norm.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Norm.Database.EntityTypeConfigurations
{
    class GuildModerationAuditRecordETC : IEntityTypeConfiguration<GuildModerationAuditRecord>
    {
        public void Configure(EntityTypeBuilder<GuildModerationAuditRecord> builder)
        {
            builder.ToTable("all_guild_moderation_audit_records");

            builder.HasKey(ar => ar.Id)
                .HasName("audit_record_id");

            builder.Property(ar => ar.GuildId)
                .HasColumnName("guild_id");

            builder.Property(ar => ar.ModeratorUserId)
                .HasColumnName("moderator_user_id");

            builder.Property(ar => ar.UserId)
                .HasColumnName("user_id");

            builder.Property(ar => ar.ModerationAction)
                .HasColumnName("moderation_action");

            builder.Property(ar => ar.Reason)
                .HasColumnName("reason");

            builder.Property(ar => ar.Timestamp)
                .HasColumnName("timestamp");
        }
    }
}
