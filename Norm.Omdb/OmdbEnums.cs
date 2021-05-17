using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Norm.Services.Omdb
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

    public enum OmdbPlotOption
    {
        SHORT,
        FULL,
    }

    public static class OmdbPlotOptionExtensions
    {
        public static string ToQueryValue(this OmdbPlotOption option)
            => option switch
            {
                OmdbPlotOption.SHORT => "short",
                OmdbPlotOption.FULL => "full",
                _ => throw new NotImplementedException("An unknown OMDB plot option was used"),
            };
    }
}
