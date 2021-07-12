using System.Collections.Generic;

namespace Norm.Omdb.Types
{
    public class OmdbSearchResults
    {
        public List<OmdbItem>? Search { get; init; }
        public int? TotalResults { get; init; }
        public bool? Response { get; init; }
        public string? Error { get; init; }
    }
}
