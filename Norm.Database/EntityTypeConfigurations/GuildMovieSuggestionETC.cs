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

            builder.HasKey(b => b.ImdbId)
                .HasName("imdb_id");

            builder.Property(b => b.GuildId)
                .HasColumnName("guild_id");

            builder.Property(b => b.Rating)
                .HasColumnName("rating");

            builder.Property(b => b.SuggesterId)
                .HasColumnName("suggester_id");

            builder.Property(b => b.Title)
                .HasColumnName("title");

            builder.HasMany(b => b.AssociatedGuildMovieNights)
                .WithMany(b => b.AssociatedMovieSuggestions)
                .UsingEntity<MovieNightAndSuggestion>(
                    j => j.HasOne(x => x.MovieNight)
                        .WithMany(x => x.MovieNightAndSuggestions)
                        .HasForeignKey(x => x.MovieNightId)
                        .OnDelete(DeleteBehavior.NoAction),
                    j => j.HasOne(x => x.MovieSuggestion)
                        .WithMany(x => x.MovieNightAndSuggestions)
                        .HasForeignKey(x => x.MovieSuggestionId)
                        .OnDelete(DeleteBehavior.NoAction),
                    j =>
                    {
                        j.Property(x => x.EmojiId).HasColumnName("emoji_id");
                        j.HasKey(x => new { x.MovieNightId, x.MovieSuggestionId });
                    }
                ); ;
        }
    }
}
