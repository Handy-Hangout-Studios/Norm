using Harold.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harold.Database.EntityConfigurations
{
    public class GuildNovelRegistrationEntityTypeConfiguration : IEntityTypeConfiguration<GuildNovelRegistration>
    {
        public void Configure(EntityTypeBuilder<GuildNovelRegistration> builder)
        {
            builder.HasKey(g => new { g.GuildId, g.NovelInfoId, g.AnnouncementChannelId });

            builder.Property(g => g.GuildId)
                .IsRequired()
                .HasColumnName("guild_id");

            builder.Property(g => g.AnnouncementChannelId)
                .IsRequired()
                .HasColumnName("announcement_channel_id");

            builder.Property(g => g.RoleId)
                .HasColumnName("role_id");

            builder.Property(g => g.NovelInfoId)
                .IsRequired()
                .HasColumnName("novel_info_id");
        }
    }
}
