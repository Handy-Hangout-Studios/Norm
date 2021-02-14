using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Norm.Database.Entities;

namespace Norm.Database.EntityTypeConfigurations
{
    internal class GuildLogsChannelETC : IEntityTypeConfiguration<GuildLogChannel>
    {
        public void Configure(EntityTypeBuilder<GuildLogChannel> builder)
        {
            builder.ToTable("all_guild_log_channels");

            builder.HasKey(g => g.Id)
                .HasName("guild_log_channel_id");

            builder.Property(g => g.GuildId)
                .HasColumnName("guild_id");

            builder.Property(g => g.ChannelId)
                .HasColumnName("log_channel_id");
        }
    }
}
