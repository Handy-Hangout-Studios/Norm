using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Norm.Database.Entities;

namespace Norm.Database.EntityTypeConfigurations
{
    internal class GuildMovieSuggestionETC : IEntityTypeConfiguration<GuildMovieSuggestion>
    {
        public void Configure(EntityTypeBuilder<GuildMovieSuggestion> builder)
        {
            builder.ToTable("all_guild_movie_suggestions");

            builder.HasKey(b => new { b.ImdbId, b.GuildId });

            builder.Property(b => b.ImdbId)
                .HasColumnName("imdb_id");

            builder.Property(b => b.GuildId)
                .HasColumnName("guild_id");

            builder.Property(b => b.SuggesterId)
                .HasColumnName("suggester_id")
                .IsRequired();

            builder.Property(b => b.Rating)
                .HasColumnName("rating")
                .IsRequired();

            builder.Property(b => b.Title)
                .HasColumnName("title")
                .IsRequired();

            builder.Property(b => b.Year)
                .HasColumnName("year")
                .IsRequired();

            builder.Property(b => b.InstantWatched)
                .HasColumnName("instant_watched")
                .IsRequired(false);

            // Configuration for the MovieNightAndSuggestions Navigation
            // Path is in the MovieNightAndSuggestionETC.cs file. 
        }
    }
}
