using Norm.Omdb.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Norm.Omdb.Types
{
    public class LazyOmdbList
    {
        private readonly List<OmdbItem> _omdbItems;
        public int Count { get; private set; } = 0;
        private readonly OmdbClient _client;
        private int _currentIndex = 0;
        private int _currentPage = 1;
        private readonly string _search;
        private readonly OmdbSearchType _resultType;
        private readonly int? _yearOfRelease;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="search"></param>
        /// <param name="resultType"></param>
        /// <param name="yearOfRelease"></param>
        internal LazyOmdbList(OmdbClient client, string search, OmdbSearchType resultType, int? yearOfRelease)
        {
            this._client = client;
            this._search = search;
            this._resultType = resultType;
            this._yearOfRelease = yearOfRelease;
            this._omdbItems = new();
        }

        internal async Task<LazyOmdbList> InitializeAsync()
        {
            OmdbSearchResults results = await this._client.InternalSearchByTitleAsync(this._search, this._resultType, this._yearOfRelease, this._currentPage);
            if (results.Search != null)
                this._omdbItems.AddRange(results.Search);

            if (results.TotalResults != null)
                this.Count = (int) results.TotalResults;

            return this;
        }

        /// <summary>
        /// Retrieve the current OmdbItem
        /// </summary>
        /// <returns>The current OmdbItem</returns>
        public OmdbItem CurrentItem()
        {
            return this._omdbItems[this._currentIndex];
        }

        /// <summary>
        /// Check if there is a next OmdbItem
        /// </summary>
        /// <returns></returns>
        public bool HasNext()
        {
            return this._currentIndex + 1 < this.Count;
        }

        /// <summary>
        /// Move to the next OmdbItem
        /// </summary>
        /// <returns></returns>
        public async Task MoveNext()
        {
            this._currentIndex += 1;
            if (this._currentIndex == this.Count)
                throw new System.Exception("Attempted to move to next and there are no further items. Please check with HasNext before using MoveNext.");

            if (this._currentIndex == this._omdbItems.Count)
            {
                this._currentPage += 1;
                OmdbSearchResults results = await this._client.InternalSearchByTitleAsync(this._search, this._resultType, this._yearOfRelease, this._currentPage);
                if (results.Response.HasValue && !results.Response.Value)
                {
                    this._currentPage -= 1;
                    this._currentIndex -= 1;
                    throw new System.Exception("Attempted to move to next and there was no further pages. Please check with HasNext before using MoveNext.");
                }

                if (results.Search != null)
                    this._omdbItems.AddRange(results.Search);
            }
        }

        /// <summary>
        /// Check if there is a previous OmdbItem
        /// </summary>
        /// <returns></returns>
        public bool HasPrev()
        {
            return this._currentIndex - 1 >= 0;
        }

        /// <summary>
        /// Move to the previous OmdbItem
        /// </summary>
        /// <returns></returns>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task MovePrev()
        {
            this._currentIndex -= 1;
            if (this._currentIndex < 0)
                throw new System.Exception("Attempted to move to previous and there are no previous items. Please check with HasPrev before using MovePrev");
        }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    }
}
