using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Norm.Database.Entities
{
    public class MovieNightAndSuggestion
    {
        public ulong EmojiId { get; set; }
        public ulong MovieNightId { get; set; }
        public GuildMovieNight MovieNight { get; set; }
        public string MovieSuggestionId { get; set; }
        public GuildMovieSuggestion MovieSuggestion { get; set; }
    }
}
