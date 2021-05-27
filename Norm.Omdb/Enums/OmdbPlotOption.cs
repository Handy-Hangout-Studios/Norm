using System;

namespace Norm.Omdb.Enums
{
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
