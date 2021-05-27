using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Norm.Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Norm.Database.EntityTypeConfigurations
{
    public class MovieNightAndSuggestionETC : IEntityTypeConfiguration<MovieNightAndSuggestion>
    {
        public void Configure(EntityTypeBuilder<MovieNightAndSuggestion> builder)
        {
            builder.ToTable("movie_night_and_suggestion_join");

            builder.HasKey(mns => new { mns.MovieNightId, mns.MovieSuggestionId });

            builder.Property(mns => mns.EmojiId)
                .HasColumnName("emoji_id")
                .IsRequired();

            builder.HasOne(mns => mns.MovieNight)
                .WithMany(mn => mn.MovieNightAndSuggestions)
                .HasForeignKey(mns => mns.MovieNightId);

            builder.HasOne(mns => mns.MovieSuggestion)
                .WithMany(ms => ms.MovieNightAndSuggestions)
                .HasForeignKey(mns => mns.MovieSuggestionId);
        }
    }
}
