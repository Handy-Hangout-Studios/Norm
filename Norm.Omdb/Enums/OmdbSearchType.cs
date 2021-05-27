using System;

namespace Norm.Omdb.Enums
{
    public enum OmdbSearchType
    {
        MOVIE,
        SERIES,
        EPISODE,
        GAME,
        NONE,
    }

    public static class OmdbSearchTypeExtensions
    {
        public static string ToQueryValue(this OmdbSearchType type)
            => type switch
            {
                OmdbSearchType.MOVIE => "movie",
                OmdbSearchType.SERIES => "series",
                OmdbSearchType.EPISODE => "episode",
                OmdbSearchType.GAME => "game",
                _ => throw new NotImplementedException("An unknown OMDB search type was used"),
            };
    }
}
