using Norm.Omdb.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Norm.Database.Entities
{
    public class GuildMovieNight
    {
        public ulong Id { get; set; }
        public string VotingStartHangfireId { get; set; }
        public string VotingEndHangfireId { get; set; }
        public string MovieNightStartHangfireId { get; set; }
        public int NumberOfSuggestions { get; set; }
        public OmdbParentalRating MaximumRating { get; set; } 
        public ulong AnnouncementChannelId { get; set; }
        public ulong VotingMessageId { get; set; }
        public ICollection<GuildMovieSuggestion> AssociatedMovieSuggestions { get; set; }
        public List<MovieNightAndSuggestion> MovieNightAndSuggestions { get; set; }
        public Dictionary<ulong, string> EmojisToMovieSuggestion { get; set; }
        public int? SelectedMovieIndex { get; set; }
        public ulong GuildId { get; set; }
        public ulong HostId { get; set; }
    }
}
