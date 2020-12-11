using Harold.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harold.Database.EntityConfigurations
{
    class NovelInfoEntityTypeConfiguration : IEntityTypeConfiguration<NovelInfo>
    {
        public void Configure(EntityTypeBuilder<NovelInfo> builder)
        {
            builder.HasKey(n => n.Id)
                .HasName("id");

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
        }
    }
}
