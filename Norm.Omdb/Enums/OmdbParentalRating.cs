using System;

namespace Norm.Omdb.Enums
{
    public enum OmdbParentalRating
    {
        G = 10,
        PG = 20,
        PG_13 = 30,
        R = 40,
        NC_17 = 50,
        NR = 60,
    }

    public static class OmdbParentalRatingExtensions
    {
        public static string ToQueryValue(this OmdbParentalRating rating)
            => rating switch
            {
                OmdbParentalRating.G => "G",
                OmdbParentalRating.PG => "PG",
                OmdbParentalRating.PG_13 => "PG-13",
                OmdbParentalRating.R => "R",
                OmdbParentalRating.NC_17 => "NC-17",
                OmdbParentalRating.NR => "Not Rated",
                _ => throw new ArgumentException("Unknown OmdbParentalRating used"),
            };
    }
}
