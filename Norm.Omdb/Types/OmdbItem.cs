using System;
using Norm.Omdb.Enums;
using System.Diagnostics.CodeAnalysis;

namespace Norm.Omdb.Types
{
    public class OmdbItem
    {
        [NotNull]
        public string? Title { get; init; }
        public int Year { get; init; }
        [NotNull]
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
            return HashCode.Combine(this.Title, this.Poster, this.Type, this.Year, this.ImdbId);
        }
    }
}
