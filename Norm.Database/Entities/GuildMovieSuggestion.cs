using NodaTime;
using Norm.Omdb.Enums;
using System.Collections.Generic;

namespace Norm.Database.Entities
{
    public class GuildMovieSuggestion
    {
        public GuildMovieSuggestion(string imdbId, ulong suggesterId, string title, ulong guildId, int year, OmdbParentalRating rating)
        {
            this.ImdbId = imdbId;
            this.SuggesterId = suggesterId;
            this.Title = title;
            this.GuildId = guildId;
            this.Rating = rating;
            this.Year = year;
        }

        public string ImdbId { get; set; }
        public ulong GuildId { get; set; }
        public ulong SuggesterId { get; set; }
        public string Title { get; set; }
        public OmdbParentalRating Rating { get; set; }
        public ICollection<MovieNightAndSuggestion> MovieNightAndSuggestions { get; set; } = null!;
        public int Year { get; set; }
        public Instant? InstantWatched { get; set; }
    }
}
