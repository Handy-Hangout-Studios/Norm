namespace Norm.Database.Entities
{
    public class MovieNightAndSuggestion
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="movieNightId"></param>
        /// <param name="movieSuggestionId"></param>
        /// <param name="emojiId">Expects a default Discord Emoji with a Unicode representation</param>
        /// <param name="guildId"></param>
        public MovieNightAndSuggestion(int movieNightId, string movieSuggestionId, string emojiId, ulong guildId)
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
        public string EmojiId { get; set; }

        // Navigation Paths
        public GuildMovieNight MovieNight { get; set; } = null!;
        public GuildMovieSuggestion MovieSuggestion { get; set; } = null!;
    }
}
