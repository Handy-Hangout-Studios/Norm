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
        public MovieNightAndSuggestion(ulong movieNightId, string movieSuggestionId, ulong emojiId)
        {
            this.MovieNightId = movieNightId;
            this.MovieSuggestionId = movieSuggestionId;
            this.EmojiId = emojiId;
        }

        // Composite key
        public ulong MovieNightId { get; set; }
        public string MovieSuggestionId { get; set; }
        
        // Data
        public ulong EmojiId { get; set; }

        // Navigation Paths
        public GuildMovieNight MovieNight { get; set; } = null!;
        public GuildMovieSuggestion MovieSuggestion { get; set; } = null!;
    }
}
