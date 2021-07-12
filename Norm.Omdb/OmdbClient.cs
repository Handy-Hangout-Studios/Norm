using Microsoft.Extensions.DependencyInjection;
using Norm.Omdb.Enums;
using Norm.Omdb.JsonConverters;
using Norm.Omdb.Types;
using RestSharp;
using RestSharp.Serializers.SystemTextJson;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Norm.Omdb
{
    public class OmdbClient
    {
        private readonly RestClient restClient;
        private readonly OmdbClientOptions options;

        public OmdbClient(OmdbClientOptions options)
        {
            this.restClient = new RestClient("http://www.omdbapi.com/");

            JsonSerializerOptions stjOptions = new()
            {
                PropertyNameCaseInsensitive = true
            };
            stjOptions.Converters.Add(new LocalDateJsonConverter());
            stjOptions.Converters.Add(new OmdbParentalRatingJsonConverter());
            stjOptions.Converters.Add(new OmdbSearchTypeJsonConverter());
            stjOptions.Converters.Add(new Int32JsonConverter());
            stjOptions.Converters.Add(new BooleanJsonConverter());

            this.restClient.UseSystemTextJson(stjOptions);
            this.options = new OmdbClientOptions
            {
                ApiKey = options.ApiKey ?? throw new Exception("A null api key was used in the config"),
                Version = options.Version ?? 1,
            };
        }

        public async Task<OmdbMovie> GetByImdbIdAsync(
            string imdbId, OmdbSearchType resultType = OmdbSearchType.NONE,
            int? yearOfRelease = null, OmdbPlotOption omdbPlotOption = OmdbPlotOption.SHORT)
            => await this.GetByIdOrTitleAsync(imdbId, null, resultType, yearOfRelease, omdbPlotOption);

        public async Task<OmdbMovie> GetByMovieTitleAsync(
            string title, OmdbSearchType resultType = OmdbSearchType.NONE,
            int? yearOfRelease = null, OmdbPlotOption omdbPlotOption = OmdbPlotOption.SHORT)
            => await this.GetByIdOrTitleAsync(null, title, resultType, yearOfRelease, omdbPlotOption);

        private async Task<OmdbMovie> GetByIdOrTitleAsync(
            string? imdbId,
            string? title,
            OmdbSearchType resultType = OmdbSearchType.NONE,
            int? yearOfRelease = null,
            OmdbPlotOption omdbPlotOption = OmdbPlotOption.SHORT)
        {
            if (imdbId is null && title is null)
                throw new ArgumentException("Both imdbId and title are null which is not allowed");

            RestRequest request = new(Method.GET);
            this.AddDefaultParameters(request);

            if (imdbId is not null)
                request.AddParameter("i", imdbId);

            if (title is not null)
                request.AddParameter("t", title);

            if (resultType is not OmdbSearchType.NONE)
                request.AddParameter("type", resultType.ToQueryValue());

            if (yearOfRelease is not null)
                request.AddParameter("y", yearOfRelease);

            request.AddParameter("plot", omdbPlotOption.ToQueryValue());

            return await this.restClient.GetAsync<OmdbMovie>(request);
        }

        public async Task<LazyOmdbList> SearchByTitleAsync(
            string search,
            OmdbSearchType resultType = OmdbSearchType.NONE,
            int? yearOfRelease = null
            )
        {
            return await new LazyOmdbList(this, search, resultType, yearOfRelease).InitializeAsync();
        }

        internal async Task<OmdbSearchResults> InternalSearchByTitleAsync(
            string search,
            OmdbSearchType resultType = OmdbSearchType.NONE,
            int? yearOfRelease = null,
            int pageNumber = 1)
        {
            RestRequest request = new(Method.GET);
            this.AddDefaultParameters(request);

            request.AddParameter("s", search);
            request.AddParameter("page", pageNumber);

            if (resultType is not OmdbSearchType.NONE)
                request.AddParameter("type", resultType.ToQueryValue());

            if (yearOfRelease is not null)
                request.AddParameter("y", yearOfRelease);

            return await this.restClient.GetAsync<OmdbSearchResults>(request);
        }

        private void AddDefaultParameters(RestRequest request)
        {
            request.AddParameter("r", "json");
            request.AddParameter("v", this.options.Version!);
            request.AddParameter("apikey", this.options.ApiKey!);
        }
    }

    public static class OmdbClientServiceCollectionExtensions
    {
        public static IServiceCollection AddOmdbClient(this IServiceCollection services, Action<IServiceProvider, OmdbClientOptions> optionsBuilder)
        {
            return services.AddTransient(s =>
            {
                OmdbClientOptions options = new();
                optionsBuilder(s, options);
                return new OmdbClient(options);
            });
        }
    }
}
