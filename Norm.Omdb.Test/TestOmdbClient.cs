using Norm.Omdb.Enums;
using Norm.Omdb.Types;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Norm.Omdb.Test
{
    // These are not expected to remain correct

    [TestFixture]
    public class TestOmdbClient
    {
        private OmdbClient _client;

        [SetUp]
        public void SetUp()
        {
            this._client = new OmdbClient(new OmdbClientOptions
            {
                ApiKey = TestContext.Parameters["omdbApiKey"]
            });
        }

        [Test, TestCaseSource(nameof(_getTestSource))]
        public async Task GetByImdbIdFeatureWithSpecifiedId(string imdbId, string title, OmdbPlotOption plotOption, OmdbMovie expected)
        {
            OmdbMovie result = await this._client.GetByImdbIdAsync(imdbId, omdbPlotOption: plotOption);
            Assert.AreEqual(expected, result, $"Expected: \n{expected.ToDetailedString()}\nActual: \n{result.ToDetailedString()}\n");
        }

        [Test, TestCaseSource(nameof(_getTestSource))]
        public async Task GetByMovieTitleFeatureWithSpecifiedTitle(string imdbId, string title, OmdbPlotOption plotOption, OmdbMovie expected)
        {
            OmdbMovie result = await this._client.GetByMovieTitleAsync(title, omdbPlotOption: plotOption);
            Assert.AreEqual(expected, result, $"Expected: \n{expected.ToDetailedString()}\nActual: \n{result.ToDetailedString()}\n");
        }

        private static readonly object[] _getTestSource = new object[]
        {
            new object[] {
                "tt0389790",
                "bee movie",
                OmdbPlotOption.SHORT,
                new OmdbMovie {
                    Title = "Bee Movie",
                    Year = 2007,
                    Rated = OmdbParentalRating.PG,
                    Released = new NodaTime.LocalDate(2007, 11, 2),
                    Runtime = "91 min",
                    Genre = "Animation, Adventure, Comedy, Family",
                    Director = "Simon J. Smith, Steve Hickner",
                    Writer = "Jerry Seinfeld, Spike Feresten, Barry Marder, Andy Robin, Chuck Martin (additional screenplay material), Tom Papa (additional screenplay material)",
                    Actors = "Jerry Seinfeld, Renée Zellweger, Matthew Broderick, Patrick Warburton",
                    Plot =  "Barry B. Benson, a bee just graduated from college, is disillusioned at his lone career choice: making honey. On a special trip outside the hive, Barry's life is saved by Vanessa, a florist in New York City. As their relationship blossoms, he discovers humans actually eat honey, and subsequently decides to sue them.",
                    Language = "English",
                    Country = "USA",
                    Awards = "Nominated for 1 Golden Globe. Another 1 win & 14 nominations.",
                    Poster = "https://m.media-amazon.com/images/M/MV5BMjE1MDYxOTA4MF5BMl5BanBnXkFtZTcwMDE0MDUzMw@@._V1_SX300.jpg",
                    Ratings = new List<Dictionary<string, string>>
                    {
                        new Dictionary<string, string> {
                            { "Source", "Internet Movie Database"},
                            { "Value", "6.1/10"}
                        },
                        new Dictionary<string, string> {
                            { "Source", "Rotten Tomatoes"},
                            { "Value", "49%"}
                        },
                        new Dictionary<string, string> {
                            { "Source", "Metacritic"},
                            { "Value", "54/100"}
                        }
                    },
                    Metascore = "54",
                    ImdbRating = "6.1",
                    ImdbVotes = "145,569",
                    ImdbId = "tt0389790",
                    Type = OmdbSearchType.MOVIE,
                    DVD = "25 Nov 2015",
                    BoxOffice = "$126,631,277",
                    Production = "DreamWorks SKG",
                    Website = "N/A",
                    Response = true,
                },
            },
            new object[] {
                "tt0389790",
                "bee movie",
                OmdbPlotOption.FULL,
                new OmdbMovie {
                    Title = "Bee Movie",
                    Year = 2007,
                    Rated = OmdbParentalRating.PG,
                    Released = new NodaTime.LocalDate(2007, 11, 2),
                    Runtime = "91 min",
                    Genre = "Animation, Adventure, Comedy, Family",
                    Director = "Simon J. Smith, Steve Hickner",
                    Writer = "Jerry Seinfeld, Spike Feresten, Barry Marder, Andy Robin, Chuck Martin (additional screenplay material), Tom Papa (additional screenplay material)",
                    Actors = "Jerry Seinfeld, Renée Zellweger, Matthew Broderick, Patrick Warburton",
                    Plot = "When the bee Barry B. Benson graduates from college, he finds that he will have only one job for his entire life, and absolutely disappointed, he joins the team responsible for bringing the honey and pollination of the flowers to visit the world outside the hive. Once in Manhattan, he is saved by the florist Vanessa and he breaks the bee law to thank Vanessa. They become friends and Barry discovers that humans exploit bees to sell the honey they produce. Barry decides to sue the human race, with destructive consequences to nature.",
                    Language = "English",
                    Country = "USA",
                    Awards = "Nominated for 1 Golden Globe. Another 1 win & 14 nominations.",
                    Poster = "https://m.media-amazon.com/images/M/MV5BMjE1MDYxOTA4MF5BMl5BanBnXkFtZTcwMDE0MDUzMw@@._V1_SX300.jpg",
                    Ratings = new List<Dictionary<string, string>>
                    {
                        new Dictionary<string, string> {
                            { "Source", "Internet Movie Database"},
                            { "Value", "6.1/10"}
                        },
                        new Dictionary<string, string> {
                            { "Source", "Rotten Tomatoes"},
                            { "Value", "49%"}
                        },
                        new Dictionary<string, string> {
                            { "Source", "Metacritic"},
                            { "Value", "54/100"}
                        }
                    },
                    Metascore = "54",
                    ImdbRating = "6.1",
                    ImdbVotes = "145,569",
                    ImdbId = "tt0389790",
                    Type = OmdbSearchType.MOVIE,
                    DVD = "25 Nov 2015",
                    BoxOffice = "$126,631,277",
                    Production = "DreamWorks SKG",
                    Website = "N/A",
                    Response = true,
                },
            },
            new object[] {
                "tt0095489",
                "the land before time",
                OmdbPlotOption.SHORT,
                new OmdbMovie {
                    Title = "The Land Before Time",
                    Year = 1988,
                    Rated = OmdbParentalRating.G,
                    Released = new NodaTime.LocalDate(1988, 11, 18),
                    Runtime = "69 min",
                    Genre = "Animation, Adventure, Drama, Family",
                    Director = "Don Bluth",
                    Writer = "Stu Krieger (screenplay), Judy Freudberg (story), Tony Geiss (story)",
                    Actors = "Judith Barsi, Pat Hingle, Gabriel Damon, Helen Shaver",
                    Plot = "An orphaned brontosaurus teams up with other young dinosaurs in order to reunite with their families in a valley.",
                    Language = "English",
                    Country = "USA, Ireland",
                    Awards = "2 nominations.",
                    Poster = "https://m.media-amazon.com/images/M/MV5BNDVhZjVmZWYtYTE0OC00MGFjLWI1YWQtZmJhNmE5NzI4ZWE4XkEyXkFqcGdeQXVyMzczMzE2ODM@._V1_SX300.jpg",
                    Ratings = new List<Dictionary<string, string>>
                    {
                        new Dictionary<string, string> {
                            { "Source", "Internet Movie Database"},
                            { "Value", "7.4/10"}
                        },
                        new Dictionary<string, string> {
                            { "Source", "Rotten Tomatoes"},
                            { "Value", "70%"}
                        },
                        new Dictionary<string, string> {
                            { "Source", "Metacritic"},
                            { "Value", "66/100"}
                        }
                    },
                    Metascore = "66",
                    ImdbRating = "7.4",
                    ImdbVotes = "81,043",
                    ImdbId = "tt0095489",
                    Type = OmdbSearchType.MOVIE,
                    DVD = "25 May 2015",
                    BoxOffice = "$48,092,846",
                    Production = "Universal Pictures, Lucasfilm Ltd., Amblin Entertainment",
                    Website = "N/A",
                    Response = true,
                },
            },
            new object[] {
                "tt0095489",
                "the land before time",
                OmdbPlotOption.FULL,
                new OmdbMovie {
                    Title = "The Land Before Time",
                    Year = 1988,
                    Rated = OmdbParentalRating.G,
                    Released = new NodaTime.LocalDate(1988, 11, 18),
                    Runtime = "69 min",
                    Genre = "Animation, Adventure, Drama, Family",
                    Director = "Don Bluth",
                    Writer = "Stu Krieger (screenplay), Judy Freudberg (story), Tony Geiss (story)",
                    Actors = "Judith Barsi, Pat Hingle, Gabriel Damon, Helen Shaver",
                    Plot = "An orphaned brontosaurus named Littlefoot sets off in search of the legendary Great Valley. A land of lush vegetation where the dinosaurs can thrive and live in peace. Along the way he meets four other young dinosaurs, each one a different species, and they encounter several obstacles as they learn to work together in order to survive.",
                    Language = "English",
                    Country = "USA, Ireland",
                    Awards = "2 nominations.",
                    Poster = "https://m.media-amazon.com/images/M/MV5BNDVhZjVmZWYtYTE0OC00MGFjLWI1YWQtZmJhNmE5NzI4ZWE4XkEyXkFqcGdeQXVyMzczMzE2ODM@._V1_SX300.jpg",
                    Ratings = new List<Dictionary<string, string>>
                    {
                        new Dictionary<string, string> {
                            { "Source", "Internet Movie Database"},
                            { "Value", "7.4/10"}
                        },
                        new Dictionary<string, string> {
                            { "Source", "Rotten Tomatoes"},
                            { "Value", "70%"}
                        },
                        new Dictionary<string, string> {
                            { "Source", "Metacritic"},
                            { "Value", "66/100"}
                        }
                    },
                    Metascore = "66",
                    ImdbRating = "7.4",
                    ImdbVotes = "81,043",
                    ImdbId = "tt0095489",
                    Type = OmdbSearchType.MOVIE,
                    DVD = "25 May 2015",
                    BoxOffice = "$48,092,846",
                    Production = "Universal Pictures, Lucasfilm Ltd., Amblin Entertainment",
                    Website = "N/A",
                    Response = true,
                }
            }
        };

        [Test, TestCaseSource(nameof(_searchTestSource))]
        public async Task SearchWithTextFeature(string search, List<OmdbItem> expected)
        {
            LazyOmdbList omdbList = await this._client.SearchByTitleAsync(search);
            List<OmdbItem> resultList = new() { omdbList.CurrentItem() };
            while (omdbList.HasNext())
            {
                await omdbList.MoveNext();
                resultList.Add(omdbList.CurrentItem());
            }
            Assert.IsTrue(resultList.SequenceEqual(expected));
        }

        private static readonly object[] _searchTestSource = new object[]
        {
            new object[]
            {
                "remember the to",
                new List<OmdbItem>
                {
                    new OmdbItem
                    {
                        Title = "The Witcher 3: Wild Hunt - A Night to Remember",
                        Year = 2015,
                        ImdbId = "tt5091902",
                        Type = OmdbSearchType.MOVIE,
                        Poster = "https://m.media-amazon.com/images/M/MV5BNDU4YzJjMTAtMzBlOS00ZTBmLWEyNGYtZjM0NDkzNzg0ZDMwXkEyXkFqcGdeQXVyNTU0NDgwMzA@._V1_SX300.jpg"
                    },
                    new OmdbItem
                    {
                        Title = "Bob the Builder: A Christmas to Remember",
                        Year = 2001,
                        ImdbId = "tt0305312",
                        Type = OmdbSearchType.MOVIE,
                        Poster = "https://m.media-amazon.com/images/M/MV5BMjc3NzA1MTM4MV5BMl5BanBnXkFtZTgwODY2OTk1MDE@._V1_SX300.jpg"
                    },
                    new OmdbItem
                    {
                        Title = "The Nanny Reunion: A Nosh to Remember",
                        Year = 2004,
                        ImdbId = "tt0420793",
                        Type = OmdbSearchType.MOVIE,
                        Poster = "https://m.media-amazon.com/images/M/MV5BMWMyYTNhNzYtN2ZkNi00MTk0LWIyMDYtY2Y1YzNlZjJkZWY4XkEyXkFqcGdeQXVyMTkzODUwNzk@._V1_SX300.jpg"
                    },
                    new OmdbItem
                    {
                        Title = "The Ten Commandments Number 3: Remember to Keep Holy the Sabbath Day",
                        Year = 1995,
                        ImdbId = "tt0373359",
                        Type = OmdbSearchType.MOVIE,
                        Poster = "N/A"
                    },
                    new OmdbItem
                    {
                        Title = "The Making of 'A Night to Remember'",
                        Year = 1993,
                        ImdbId = "tt0403259",
                        Type = OmdbSearchType.MOVIE,
                        Poster = "N/A"
                    },
                    new OmdbItem
                    {
                        Title = "A Walk to Remember: A Day on the Set with Mandy Moore",
                        Year = 2002,
                        ImdbId = "tt3921080",
                        Type = OmdbSearchType.MOVIE,
                        Poster = "https://m.media-amazon.com/images/M/MV5BMTM1MTYzNjA3M15BMl5BanBnXkFtZTcwMjk2NTE4Mg@@._V1_SX300.jpg"
                    },
                    new OmdbItem
                    {
                        Title = "Try to Remember: The Fantasticks",
                        Year = 2003,
                        ImdbId = "tt0401846",
                        Type = OmdbSearchType.MOVIE,
                        Poster = "https://m.media-amazon.com/images/M/MV5BMjA4NjIxMzIwOF5BMl5BanBnXkFtZTcwNjczNjcyMQ@@._V1_SX300.jpg"
                    },
                    new OmdbItem
                    {
                        Title = "Dreams to Remember (The Legacy of Otis Redding)",
                        Year = 2007,
                        ImdbId = "tt2163668",
                        Type = OmdbSearchType.MOVIE,
                        Poster = "https://m.media-amazon.com/images/M/MV5BNzRhZjhjYmYtY2JiMi00YzM2LWE2MDktODQyNThiMTgyZjcyXkEyXkFqcGdeQXVyMTU1MjY3NzU@._V1_SX300.jpg"
                    },
                    new OmdbItem
                    {
                        Title = "Do You Remember When We Used to Go to the Sea?",
                        Year = 2012,
                        ImdbId = "tt3284340",
                        Type = OmdbSearchType.MOVIE,
                        Poster = "N/A"
                    },
                    new OmdbItem
                    {
                        Title = "CBS Presents: A New York Christmas to Remember at St. Paul the Apostle",
                        Year = 2013,
                        ImdbId = "tt3645838",
                        Type = OmdbSearchType.MOVIE,
                        Poster = "https://m.media-amazon.com/images/M/MV5BZjc5MWJlNGQtOTYyZi00YmU2LWJiYzgtOTc1Nzk4NGFiZjVlXkEyXkFqcGdeQXVyNDM3OTUzMjY@._V1_SX300.jpg"
                    },
                    new OmdbItem
                    {
                        Title = "Remember to High Five the Salesman",
                        Year = 2015,
                        ImdbId = "tt3698266",
                        Type = OmdbSearchType.MOVIE,
                        Poster = "https://m.media-amazon.com/images/M/MV5BMjM2OTM1ODA1MV5BMl5BanBnXkFtZTgwNDAwNjA5NTE@._V1_SX300.jpg"
                    },
                    new OmdbItem
                    {
                        Title = "Wanktoff and Beaton: Remember the Sabbath, to Keep It Holy",
                        Year = 2014,
                        ImdbId = "tt4172422",
                        Type = OmdbSearchType.MOVIE,
                        Poster = "N/A"
                    },
                    new OmdbItem
                    {
                        Title = "Scotch on the Rocks to Remember, Black Coffee to Forget",
                        Year = 1976,
                        ImdbId = "tt0387568",
                        Type = OmdbSearchType.MOVIE,
                        Poster = "N/A"
                    },
                    new OmdbItem
                    {
                        Title = "The Making of... 'Kenny & Dolly: A Christmas to Remember'",
                        Year = 1984,
                        ImdbId = "tt0810005",
                        Type = OmdbSearchType.MOVIE,
                        Poster = "N/A"
                    },
                    new OmdbItem
                    {
                        Title = "The Last to Remember",
                        Year = 2008,
                        ImdbId = "tt1244583",
                        Type = OmdbSearchType.MOVIE,
                        Poster = "N/A"
                    },
                    new OmdbItem
                    {
                        Title = "III. Remember the Sabbath Day to Keep It Holy",
                        Year = 2008,
                        ImdbId = "tt1250770",
                        Type = OmdbSearchType.MOVIE,
                        Poster = "N/A"
                    },
                    new OmdbItem
                    {
                        Title = "The Instruction: To Forget You're Falling in Love Just Try to Remember It as Much as You Ca",
                        Year = 2014,
                        ImdbId = "tt5211948",
                        Type = OmdbSearchType.MOVIE,
                        Poster = "N/A"
                    },
                    new OmdbItem
                    {
                        Title = "Scotch on the Rocks to Remember, Black Coffee to Forget",
                        Year = 2016,
                        ImdbId = "tt5319170",
                        Type = OmdbSearchType.MOVIE,
                        Poster = "N/A"
                    },
                    new OmdbItem
                    {
                        Title = "An Affair to Remember: On the Set of 'Unfaithful'",
                        Year = 2002,
                        ImdbId = "tt5492244",
                        Type = OmdbSearchType.MOVIE,
                        Poster = "N/A"
                    },
                    new OmdbItem
                    {
                        Title = "A Deadly Affair to Remember II: The Final Fight",
                        Year = 2018,
                        ImdbId = "tt6382772",
                        Type = OmdbSearchType.MOVIE,
                        Poster = "https://m.media-amazon.com/images/M/MV5BYmY4MzExYjctN2JhMC00MzU2LWE1N2UtMzM2ODhkYzIzYTUwXkEyXkFqcGdeQXVyNjE5MTEyNjE@._V1_SX300.jpg"
                    },
                    new OmdbItem
                    {
                        Title = "The Holiday to Remember",
                        Year = 2017,
                        ImdbId = "tt6422272",
                        Type = OmdbSearchType.MOVIE,
                        Poster = "https://m.media-amazon.com/images/M/MV5BZTA1ZTcwNzAtYmFhOS00OTMxLWE5YjgtODQxM2QwZTBiOTM5L2ltYWdlXkEyXkFqcGdeQXVyNTY2MDA2OTE@._V1_SX300.jpg"
                    },
                    new OmdbItem
                    {
                        Title = "The Night to Remember on 21st Street",
                        Year = 2018,
                        ImdbId = "tt7171808",
                        Type = OmdbSearchType.MOVIE,
                        Poster = "N/A"
                    },
                    new OmdbItem
                    {
                        Title = "Those Who Do Not Remember the Past Are Condemned to Repeat It",
                        Year = 2020,
                        ImdbId = "tt13976890",
                        Type = OmdbSearchType.MOVIE,
                        Poster = "N/A"
                    }
                }
            }
        };
    }
}