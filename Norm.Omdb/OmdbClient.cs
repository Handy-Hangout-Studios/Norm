using RestSharp;
using RestSharp.Serializers.SystemTextJson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Norm.Services.Omdb
{
    public class OmdbClient
    {
        private RestClient restClient;

        public OmdbClient()
        {
            this.restClient = new RestClient("http://http://www.omdbapi.com/");
            this.restClient.UseSystemTextJson(new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        public OmdbMovie? SearchByImdbId(
            string imdbId, OmdbSearchType resultType = OmdbSearchType.NONE,
            int? yearOfRelease = null, OmdbPlotOption omdbPlotOption = OmdbPlotOption.SHORT)
            => this.SearchByIdOrTitle(imdbId, null, resultType, yearOfRelease, omdbPlotOption);

        public OmdbMovie? SearchByMovieTitle(
            string title, OmdbSearchType resultType = OmdbSearchType.NONE,
            int? yearOfRelease = null, OmdbPlotOption omdbPlotOption = OmdbPlotOption.SHORT)
            => this.SearchByIdOrTitle(null, title, resultType, yearOfRelease, omdbPlotOption);

        private OmdbMovie? SearchByIdOrTitle(
            string? imdbId,
            string? title,
            OmdbSearchType resultType = OmdbSearchType.NONE,
            int? yearOfRelease = null,
            OmdbPlotOption omdbPlotOption = OmdbPlotOption.SHORT)
        {
            if (imdbId is null && title is null)
                throw new ArgumentException("Both imdbId and title are null which is not allowed");

            RestRequest request = new RestRequest(Method.GET);
            if (imdbId is not null)
                request.AddParameter("i", imdbId);
            if (title is not null)
                request.AddParameter("t", title);
            if (resultType is not OmdbSearchType.NONE)
                request.AddParameter("type", resultType.ToQueryValue());
            if (yearOfRelease is not null)
                request.AddParameter("y", yearOfRelease);
            request.AddParameter("plot", omdbPlotOption.ToQueryValue());
            request.AddParameter("r", "json");
            request.AddParameter("v", 1);
            this.restClient.GetAsync<OmdbMovie>(request);
        }
    }
}
