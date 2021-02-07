using Norm.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Norm.Database.EntityTypeConfigurations
{
    class UserTimeZoneETC : IEntityTypeConfiguration<UserTimeZone>
    {
        public void Configure(EntityTypeBuilder<UserTimeZone> builder)
        {
            builder.ToTable("all_user_time_zones");

            builder.HasKey(u => u.Id)
                .HasName("user_timezone_id");

            builder.Property(u => u.UserId)
                .HasColumnName("user_id");

            builder.Property(u => u.TimeZoneId)
                .HasColumnName("timezone_id");
        }
    }
}
