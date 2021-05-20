using NodaTime;
using Norm.Omdb.Enums;
using System.Collections.Generic;
using System.Linq;

namespace Norm.Omdb.Types
{
    public class OmdbMovie : OmdbItem
    {
        public OmdbParentalRating? Rated { get; init; }
        public LocalDate? Released { get; init; }
        public string? Runtime { get; init; }
        public string? Genre { get; init; }
        public string? Director { get; init; }
        public string? Writer { get; init; }
        public string? Actors { get; init; }
        public string? Plot { get; init; }
        public string? Language { get; init; }
        public string? Country { get; init; }
        public string? Awards { get; init; }
        public List<Dictionary<string, string>>? Ratings { get; init; }
        public string? Metascore { get; init; }
        public string? ImdbRating { get; init; }
        public string? ImdbVotes { get; init; }
        public string? DVD { get; init; }
        public string? BoxOffice { get; init; }
        public string? Production { get; init; }
        public string? Website { get; init; }

        public bool? Response { get; init; }
        public string? Error { get; init; }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(obj, this))
                return true;

            if (obj is not OmdbMovie movie)
                return false;

            return
                this.Title == movie.Title &&
                this.Year == movie.Year &&
                this.Rated == movie.Rated &&
                this.Released == movie.Released &&
                this.Runtime == movie.Runtime &&
                this.Genre == movie.Genre &&
                this.Director == movie.Director &&
                this.Actors == movie.Actors &&
                this.Plot == movie.Plot &&
                this.Language == movie.Language &&
                this.Country == movie.Country &&
                this.Awards == movie.Awards &&
                this.Poster == movie.Poster &&
                CheckListDictionaryEquality(this.Ratings, movie.Ratings) &&
                this.Metascore == movie.Metascore &&
                this.ImdbRating == movie.ImdbRating &&
                this.ImdbVotes == movie.ImdbVotes &&
                this.ImdbId == movie.ImdbId &&
                this.Type == movie.Type &&
                this.DVD == movie.DVD &&
                this.BoxOffice == movie.BoxOffice &&
                this.Production == movie.Production &&
                this.Website == movie.Website &&
                this.Response == movie.Response &&
                this.Error == movie.Error;
        }

        private static bool CheckListDictionaryEquality(List<Dictionary<string, string>>? list1, List<Dictionary<string, string>>? list2)
        {
            if (list1 == null || list2 == null)
                return (list1 == null || !list1.Any()) && (list2 == null || !list2.Any());

            if (list1.Count != list2.Count)
                return false;

            foreach ((var dict1, var dict2) in list1.Zip(list2))
            {
                foreach ((string key, string value) in dict1)
                {
                    if (!dict2.ContainsKey(key) || dict2[key] != value)
                        return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public string ToDetailedString()
        {
            return
$@"
{nameof(this.Title)}: {this.Title}
{nameof(this.Year)}: {this.Year}
{nameof(this.Rated)}: {this.Rated}
{nameof(this.Released)}: {this.Released.GetValueOrDefault().ToString("dd MMM yyyy", null)}
{nameof(this.Runtime)}: {this.Runtime}
{nameof(this.Genre)}: {this.Genre}
{nameof(this.Director)}: {this.Director}
{nameof(this.Writer)}: {this.Writer}
{nameof(this.Actors)}: {this.Actors}
{nameof(this.Plot)}: {this.Plot}
{nameof(this.Language)}: {this.Language}
{nameof(this.Country)}: {this.Country}
{nameof(this.Awards)}: {this.Awards}
{nameof(this.Poster)}: {this.Poster}
{nameof(this.Ratings)}: {GenerateRatingsString()}
{nameof(this.Metascore)}: {this.Metascore}
{nameof(this.ImdbRating)}: {this.ImdbRating}
{nameof(this.ImdbVotes)}: {this.ImdbVotes}
{nameof(this.ImdbId)}: {this.ImdbId}
{nameof(this.Type)}: {this.Type}
{nameof(this.DVD)}: {this.DVD}
{nameof(this.BoxOffice)}: {this.BoxOffice}
{nameof(this.Production)}: {this.Production}
{nameof(this.Website)}: {this.Website}
{nameof(this.Response)}: {this.Response}
{nameof(this.Error)}: {this.Error}
";
        }

        private string GenerateRatingsString()
        {
            if (this.Ratings == null)
                return string.Empty;

            return string.Join(',', this.Ratings.Select(
                dict =>
                    string.Join('|',
                    dict.Select(
                        kvp => $"{kvp.Key}: {kvp.Value}"))));
        }
    }
}
