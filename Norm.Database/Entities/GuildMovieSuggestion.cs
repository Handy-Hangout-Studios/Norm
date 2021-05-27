using Norm.Omdb.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Norm.Database.Entities
{
    public class GuildMovieSuggestion
    {
        public GuildMovieSuggestion(string imdbId, ulong suggesterId, string title, ulong guildId, OmdbParentalRating rating)
        {
            this.ImdbId = imdbId;
            this.SuggesterId = suggesterId;
            this.Title = title;
            this.GuildId = guildId;
            this.Rating = rating;
        }

        public string ImdbId { get; set; }
        public ulong GuildId { get; set; }
        public ulong SuggesterId { get; set; }
        public string Title { get; set; }
        public OmdbParentalRating  Rating { get; set; }
        public ICollection<MovieNightAndSuggestion> MovieNightAndSuggestions { get; set; } = null!;
    }
}
