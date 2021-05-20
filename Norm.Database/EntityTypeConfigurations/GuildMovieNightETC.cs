using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Norm.Database.Entities;

namespace Norm.Database.EntityTypeConfigurations
{
    public class GuildMovieNightETC : IEntityTypeConfiguration<GuildMovieNight>
    {
        public void Configure(EntityTypeBuilder<GuildMovieNight> builder)
        {
            builder.ToTable("all_guild_movie_nights");

            builder.HasKey(b => b.Id)
                .HasName("movie_night_id");

            builder.Property(b => b.VotingStartHangfireId)
                .HasColumnName("voting_start_hangfire_id");

            builder.Property(b => b.VotingEndHangfireId)
                .HasColumnName("voting_end_hangfire_id");

            builder.Property(b => b.MovieNightStartHangfireId)
                .HasColumnName("movie_night_start_hangfire_id");

            builder.Property(b => b.NumberOfSuggestions)
                .HasColumnName("number_of_suggestions");

            builder.Property(b => b.MaximumRating)
                .HasColumnName("maximum_rating");

            builder.Property(b => b.AnnouncementChannelId)
                .HasColumnName("announcement_channel_id");

            builder.Property(b => b.VotingMessageId)
                .HasColumnName("voting_message_id");

            builder.HasMany(b => b.AssociatedMovieSuggestions)
                .WithMany(b => b.AssociatedGuildMovieNights)
                .UsingEntity<MovieNightAndSuggestion>(
                    j => j.HasOne(x => x.MovieSuggestion)
                          .WithMany(x => x.MovieNightAndSuggestions)
                          .HasForeignKey(x => x.MovieSuggestionId),
                    j => j.HasOne(x => x.MovieNight)
                        .WithMany(x => x.MovieNightAndSuggestions)
                        .HasForeignKey(x => x.MovieNightId),
                    j =>
                    {
                        j.Property(x => x.EmojiId)
                            .HasColumnName("emoji_id");
                        j.HasKey(x => 
                            new { x.MovieNightId, x.MovieSuggestionId });
                    }
                );

            builder.Property(b => b.EmojisToMovieSuggestion)
                .HasColumnName("emojis_to_movie_suggestion");

            builder.Property(b => b.SelectedMovieIndex)
                .HasColumnName("selected_movie_index")
                .IsRequired(false);

            builder.Property(b => b.GuildId)
                .HasColumnName("guild_id");

            builder.Property(b => b.HostId)
                .HasColumnName("host_id");
        }
    }
}
