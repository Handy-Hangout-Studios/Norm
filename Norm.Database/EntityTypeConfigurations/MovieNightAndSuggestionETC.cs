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

            builder.Property(mns => mns.MovieNightId)
                .HasColumnName("movie_night_id");

            builder.Property(mns => mns.MovieSuggestionId)
                .HasColumnName("movie_suggestion_id");

            builder.Property(mns => mns.EmojiId)
                .HasColumnName("emoji_id")
                .IsRequired();

            builder.Property(mns => mns.GuildId)
                .HasColumnName("guild_id")
                .IsRequired();

            // System.InvalidOperationException: 'The relationship from 'MovieNightAndSuggestion.MovieSuggestion'
            // to 'GuildMovieSuggestion.MovieNightAndSuggestions' with foreign key properties
            // {'MovieSuggestionId' : string} cannot target the primary key {'ImdbId' : string, 'GuildId' : ulong}
            // because it is not compatible. Configure a principal key or a set of foreign key properties with
            // compatible types for this relationship.'


            builder.HasOne(mns => mns.MovieNight)
                .WithMany(mn => mn.MovieNightAndSuggestions)
                .HasForeignKey(mns => mns.MovieNightId);

            builder.HasOne(mns => mns.MovieSuggestion)
                .WithMany(ms => ms.MovieNightAndSuggestions)
                .HasForeignKey(mns => new { mns.MovieSuggestionId, mns.GuildId });
        }
    }
}
