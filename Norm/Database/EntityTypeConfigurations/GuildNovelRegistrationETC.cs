using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Norm.Database.Entities;

namespace Norm.Database.EntityTypeConfigurations
{
    public class GuildNovelRegistrationETC : IEntityTypeConfiguration<GuildNovelRegistration>
    {
        public void Configure(EntityTypeBuilder<GuildNovelRegistration> builder)
        {
            builder.ToTable("guild_novel_registration");

            builder.HasKey(g => new { g.GuildId, g.NovelInfoId, g.AnnouncementChannelId })
                .HasName("guild_novel_registration_id");

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

            builder.Property(g => g.MemberId)
                .HasDefaultValue(null)
                .HasColumnName("member_id");

            builder.Property(g => g.IsDm)
                .IsRequired()
                .HasDefaultValue(false)
                .HasColumnName("is_dm");

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
