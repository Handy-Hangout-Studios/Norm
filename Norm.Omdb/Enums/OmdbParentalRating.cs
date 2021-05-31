using System;
using System.Text.Json;

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



        public static OmdbParentalRating ToOmdbParentalRating(this string switcher)
        {
            return switcher.ToLower() switch
            {
                "g" => OmdbParentalRating.G,
                "tv-pg" or "pg" => OmdbParentalRating.PG,
                "approved" or "pg-13" or "tv-14" => OmdbParentalRating.PG_13,
                "r" or "tv-ma" => OmdbParentalRating.R,
                "nc-17" => OmdbParentalRating.NC_17,
                "not rated" or "n/a" or "unrated" => OmdbParentalRating.NR,
                _ => throw new JsonException($"Unknown rating read: {switcher}")
            };
        }
    }
}
