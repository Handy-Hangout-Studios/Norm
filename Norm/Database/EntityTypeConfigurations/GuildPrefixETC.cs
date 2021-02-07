using Norm.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Norm.Database.EntityTypeConfigurations
{
    class GuildPrefixETC : IEntityTypeConfiguration<GuildPrefix>
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
