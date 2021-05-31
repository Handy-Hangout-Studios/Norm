using Norm.Omdb.Enums;
using System.Collections.Generic;

namespace Norm.Database.Entities
{
    public class GuildMovieNight
    {
        public GuildMovieNight(
            string votingStartHangfireId,
            string votingEndHangfireId,
            string movieNightStartHangfireId,
            int numberOfSuggestions,
            OmdbParentalRating maximumRating,
            ulong guildId,
            ulong announcementChannelId,
            ulong hostId)
        {
            this.VotingStartHangfireId = votingStartHangfireId;
            this.VotingEndHangfireId = votingEndHangfireId;
            this.MovieNightStartHangfireId = movieNightStartHangfireId;
            this.NumberOfSuggestions = numberOfSuggestions;
            this.MaximumRating = maximumRating;
            this.GuildId = guildId;
            this.AnnouncementChannelId = announcementChannelId;
            this.HostId = hostId;
        }

        // Initial Data
        public int Id { get; set; }
        public string VotingStartHangfireId { get; set; }
        public string VotingEndHangfireId { get; set; }
        public string MovieNightStartHangfireId { get; set; }
        public int NumberOfSuggestions { get; set; }
        public OmdbParentalRating MaximumRating { get; set; }
        public ulong GuildId { get; set; }
        public ulong AnnouncementChannelId { get; set; }
        public ulong HostId { get; set; }

        // Voting Data
        public ulong? VotingMessageId { get; set; }
        public string? WinningMovieImdbId { get; set; }

        // Navigation Properties
        public ICollection<MovieNightAndSuggestion> MovieNightAndSuggestions { get; set; } = null!;
    }
}
