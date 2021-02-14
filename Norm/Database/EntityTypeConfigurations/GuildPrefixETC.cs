using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Norm.Database.Entities;

namespace Norm.Database.EntityTypeConfigurations
{
    internal class GuildPrefixETC : IEntityTypeConfiguration<GuildPrefix>
    {
        public void Configure(EntityTypeBuilder<GuildPrefix> builder)
        {
            builder.ToTable("all_guild_prefixes");

            builder.HasKey(gp => gp.Id)
                .HasName("guild_prefix_id");

            builder.Property(gp => gp.Prefix)
                .HasColumnName("prefix");

            builder.Property(gp => gp.GuildId)
                .HasColumnName("guild_id");
        }
    }
}
