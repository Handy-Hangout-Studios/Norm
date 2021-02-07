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
    public class GuildNovelRegistrationETC : IEntityTypeConfiguration<GuildNovelRegistration>
    {
        public void Configure(EntityTypeBuilder<GuildNovelRegistration> builder)
        {
            builder.ToTable("guild_novel_registration");

            builder.HasKey(g => new { g.GuildId, g.NovelInfoId, g.AnnouncementChannelId });

            builder.Property(g => g.GuildId)
                .IsRequired()
                .HasColumnName("guild_id");

            builder.Property(g => g.AnnouncementChannelId)
                .IsRequired()
                .HasColumnName("announcement_channel_id");

            builder.Property(g => g.PingEveryone)
                .IsRequired()
                .HasColumnName("ping_everyone");

            builder.Property(g => g.PingNoOne)
                .IsRequired()
                .HasColumnName("ping_no_one");

            builder.Property(g => g.RoleId)
                .HasColumnName("role_id");

            builder.Property(g => g.NovelInfoId)
                .IsRequired()
                .HasColumnName("novel_info_id");

            builder.HasOne(g => g.NovelInfo)
                .WithMany(n => n.AssociatedGuildNovelRegistrations)
                .HasForeignKey(g => g.NovelInfoId)
                .IsRequired();
        }
    }
}
