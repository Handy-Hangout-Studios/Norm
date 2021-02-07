using Norm.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Norm.Database.EntityTypeConfigurations
{
    class GuildLogsChannelETC : IEntityTypeConfiguration<GuildLogChannel>
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
