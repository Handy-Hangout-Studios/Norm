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
                .HasColumnName("voting_start_hangfire_id")
                .IsRequired();

            builder.Property(b => b.VotingEndHangfireId)
                .HasColumnName("voting_end_hangfire_id")
                .IsRequired();

            builder.Property(b => b.MovieNightStartHangfireId)
                .HasColumnName("movie_night_start_hangfire_id")
                .IsRequired();

            builder.Property(b => b.NumberOfSuggestions)
                .HasColumnName("number_of_suggestions")
                .IsRequired();

            builder.Property(b => b.MaximumRating)
                .HasColumnName("maximum_rating")
                .IsRequired();

            builder.Property(b => b.GuildId)
                .HasColumnName("guild_id")
                .IsRequired();

            builder.Property(b => b.AnnouncementChannelId)
                .HasColumnName("announcement_channel_id")
                .IsRequired();

            builder.Property(b => b.HostId)
                .HasColumnName("host_id")
                .IsRequired();

            builder.Property(b => b.VotingMessageId)
                .HasColumnName("voting_message_id")
                .IsRequired(false);

            builder.Property(b => b.SelectedMovieIndex)
                .HasColumnName("selected_movie_index")
                .IsRequired(false);

            builder.HasIndex(b => b.GuildId);

            // Configuration for the MovieNightAndSuggestions Navigation
            // Path is in the MovieNightAndSuggestionETC.cs file. 
        }
    }
}
