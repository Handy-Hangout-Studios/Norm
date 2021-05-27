using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Norm.Database.Entities
{
    public class MovieNightAndSuggestion
    {
        public MovieNightAndSuggestion(int movieNightId, string movieSuggestionId, ulong emojiId, ulong guildId)
        {
            this.MovieNightId = movieNightId;
            this.MovieSuggestionId = movieSuggestionId;
            this.EmojiId = emojiId;
            this.GuildId = guildId;
        }

        // Composite key
        public int MovieNightId { get; set; }
        public string MovieSuggestionId { get; set; }
        public ulong GuildId { get; set; }
        
        // Data
        public ulong EmojiId { get; set; }

        // Navigation Paths
        public GuildMovieNight MovieNight { get; set; } = null!;
        public GuildMovieSuggestion MovieSuggestion { get; set; } = null!;
    }
}
