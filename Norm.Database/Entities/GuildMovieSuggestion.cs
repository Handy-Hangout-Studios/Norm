using Norm.Omdb.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Norm.Database.Entities
{
    public class GuildMovieSuggestion
    {
        public string ImdbId { get; set; }
        public ulong SuggesterId { get; set; }
        public string Title { get; set; }
        public ulong GuildId { get; set; }
        public OmdbParentalRating  Rating { get; set; }
        public ICollection<GuildMovieNight> AssociatedGuildMovieNights { get; set; }
        public List<MovieNightAndSuggestion> MovieNightAndSuggestions { get; set; }
    }
}
