using Norm.Omdb.Enums;

namespace Norm.Omdb.Types
{
    public class OmdbItem
    {
        public string? Title { get; init; }
        public int? Year { get; init; }
        public string? ImdbId { get; init; }
        public OmdbSearchType Type { get; init; }
        public string? Poster { get; init; }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            if (obj is not OmdbItem item)
                return false;

            return this.Title == item.Title &&
                this.Year == item.Year &&
                this.ImdbId == item.ImdbId &&
                this.Type == item.Type &&
                this.Poster == item.Poster;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
