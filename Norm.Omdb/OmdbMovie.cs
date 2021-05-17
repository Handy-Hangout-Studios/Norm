using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Norm.Services.Omdb
{
    public class OmdbMovie
    {
        public string? Title { get; set; }
        public int? Year { get; set; }
        public ParentalRating? Rated { get; set; }
        public LocalDate? Released { get; set; }
        public string? Runtime { get; set; }
        public string? Genre { get; set; }
        public string? Director { get; set; }
        public string? Writer { get; set; }
        public string? Actors { get; set; }
        public string? Plot { get; set; }
        public string? Language { get; set; }
        public string? Country { get; set; }
        public string? Awards { get; set; }
        public string? Poster { get; set; }
        public Dictionary<string, string>? Ratings { get; set; }
        public string? Metascore { get; set; }
        public string? ImdbRating { get; set; }
        public string? ImdbVotes { get; set; }
        public string? ImdbId { get; set; }
        public OmdbSearchType? Type { get; set; }
        public string? DVD { get; set; }
        public string? BoxOffice { get; set; }
        public string? Production { get; set; }
        public string? Website { get; set; }
        public bool? Response { get; set; }
    }

    public enum ParentalRating
    {
        G = 10,
        PG = 20,
        PG_13 = 30,
        R = 40,
        X = 50,
        NR = 60,
    }
}
