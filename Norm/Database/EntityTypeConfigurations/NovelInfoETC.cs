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
    class NovelInfoETC : IEntityTypeConfiguration<NovelInfo>
    {
        public void Configure(EntityTypeBuilder<NovelInfo> builder)
        {
            builder.ToTable("novel_info");

            builder.HasKey(n => n.Id)
                .HasName("novel_info_id");

            builder.Property(n => n.FictionId)
                .HasColumnName("fiction_id")
                .IsRequired();

            builder.Property(n => n.Name)
                .HasColumnName("novel_name")
                .IsRequired();

            builder.Property(n => n.SyndicationUri)
                .HasColumnName("syndication_uri")
                .IsRequired();

            builder.Property(n => n.FictionUri)
                .HasColumnName("fiction_uri")
                .IsRequired();

            builder.Property(n => n.MostRecentChapterId)
                .HasColumnName("most_recent_chapter_id")
                .IsRequired();

            builder.HasMany(n => n.AssociatedGuildNovelRegistrations)
                .WithOne(g => g.NovelInfo)
                .HasForeignKey(g => g.NovelInfoId);
        }
    }
}
