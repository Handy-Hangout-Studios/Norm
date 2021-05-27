using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Norm.Database.Entities;

namespace Norm.Database.EntityTypeConfigurations
{
    internal class GuildWelcomeMessageSettingsETC : IEntityTypeConfiguration<GuildWelcomeMessageSettings>
    {
        public void Configure(EntityTypeBuilder<GuildWelcomeMessageSettings> builder)
        {
            builder.ToTable("all_guild_welcome_message_settings");

            builder.HasKey(gw => gw.GuildId)
                .HasName("guild_id");

            builder.Property(gw => gw.ShouldPing)
                .HasColumnName("should_ping")
                .IsRequired();

            builder.Property(gw => gw.ShouldWelcomeMembers)
                .HasColumnName("should_welcome_members")
                .IsRequired();
        }
    }
}
