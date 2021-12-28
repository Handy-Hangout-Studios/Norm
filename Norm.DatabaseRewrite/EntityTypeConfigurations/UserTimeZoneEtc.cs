using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Norm.DatabaseRewrite.Entities;

namespace Norm.DatabaseRewrite.EntityTypeConfigurations;

internal class UserTimeZoneEtc : IEntityTypeConfiguration<UserTimeZone>
{
    public void Configure(EntityTypeBuilder<UserTimeZone> builder)
    {
        builder.ToTable("all_user_time_zones");

        builder.HasKey(u => u.Id)
            .HasName("user_timezone_id");

        builder.Property(u => u.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(u => u.TimeZoneId)
            .HasColumnName("timezone_id")
            .IsRequired();

        builder.HasIndex(u => u.UserId);
    }
}