using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Norm.Database.Entities;

namespace Norm.Database.EntityTypeConfigurations
{
    internal class GuildEventETC : IEntityTypeConfiguration<GuildEvent>
    {
        public void Configure(EntityTypeBuilder<GuildEvent> builder)
        {
            builder.ToTable("all_guild_events");

            builder.HasKey(e => e.Id)
                .HasName("guild_event_id");

            builder.Property(e => e.GuildId)
                .HasColumnName("guild_id");

            builder.Property(e => e.EventName)
                .HasColumnName("event_name");

            builder.Property(e => e.EventDesc)
                .HasColumnName("event_desc");
        }
    }
}
