using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Hangfire;
using MediatR;
using Microsoft.Extensions.Options;
using Norm.Configuration;
using Norm.Database.Entities;
using Norm.Database.Requests;
using Norm.Omdb.Enums;
using Norm.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Norm.Modules
{
    [Group("test")]
    [RequireOwner]
    public partial class TestModule : BaseCommandModule
    {
        [Group("movie_night_service")]
        [Aliases("mns")]
        public class TestMovieNightGroup : BaseCommandModule
        {
            private readonly MovieNightService mns;
            private readonly IMediator mediator;
            private readonly BotOptions options;

            public TestMovieNightGroup(MovieNightService mns, IMediator mediator, IOptions<BotOptions> options)
            {
                this.mns = mns;
                this.mediator = mediator;
                this.options = options.Value;
            }

            [Command("start_voting")]
            [Aliases("sv")]
            public async Task StartVotingAsync(CommandContext context)
            {
                if (options.DevGuildId != context.Guild.Id)
                {
                    await context.RespondAsync("I'm sorry, but these commands should only be run in the bot dev guild");
                    return;
                }

                GuildMovieNight testMovieNight = await this.AddTestMovieNight(context.Guild, context.Channel, context.User);
                IEnumerable<GuildMovieSuggestion> testMovieSuggestions = await this.AddTestMovies(context.User, context.Guild);
                await this.mns.StartVoting(testMovieNight.Id);
                await this.DeleteTestMovieNight(testMovieNight);
                await this.DeleteTestMovieSuggestions(testMovieSuggestions);
            }

            [Command("calculate_votes")]
            [Aliases("cv")]
            public async Task CalculateVotesAsync(CommandContext context)
            {
                if (options.DevGuildId != context.Guild.Id)
                {
                    await context.RespondAsync("I'm sorry, but these commands should only be run in the bot dev guild");
                    return;
                }

                GuildMovieNight testMovieNight = await this.AddTestMovieNight(context.Guild, context.Channel, context.User);
                IEnumerable<GuildMovieSuggestion> testMovieSuggestions = await this.AddTestMovies(context.User, context.Guild);
                await this.mns.StartVoting(testMovieNight.Id);
                await Task.Delay(5*1000);
                await this.mns.CalculateVotes(testMovieNight.Id);
                await this.DeleteTestMovieNight(testMovieNight);
                await this.DeleteTestMovieSuggestions(testMovieSuggestions);
            }

            [Command("start_movie")]
            [Aliases("sm")]
            public async Task StartMovieAsync(CommandContext context)
            {
                if (options.DevGuildId != context.Guild.Id)
                {
                    await context.RespondAsync("I'm sorry, but these commands should only be run in the bot dev guild");
                    return;
                }

                GuildMovieNight testMovieNight = await this.AddTestMovieNight(context.Guild, context.Channel, context.User);
                IEnumerable<GuildMovieSuggestion> testMovieSuggestions = await this.AddTestMovies(context.User, context.Guild);
                await this.mns.StartVoting(testMovieNight.Id);
                await Task.Delay(15 * 1000);
                await this.mns.CalculateVotes(testMovieNight.Id);
                await Task.Delay(15 * 1000);
                await this.mns.StartMovie(testMovieNight.Id);
                await this.DeleteTestMovieNight(testMovieNight);
                await this.DeleteTestMovieSuggestions(testMovieSuggestions);
            }

            private async Task DeleteTestMovieSuggestions(IEnumerable<GuildMovieSuggestion> testMovieSuggestions)
            {
                foreach (GuildMovieSuggestion gms in testMovieSuggestions)
                {
                    await this.mediator.Send(new GuildMovieSuggestions.Delete(gms));
                }
            }

            private async Task DeleteTestMovieNight(GuildMovieNight testMovieNight)
            {
                RecurringJob.RemoveIfExists(testMovieNight.VotingStartHangfireId);
                RecurringJob.RemoveIfExists(testMovieNight.VotingEndHangfireId);
                RecurringJob.RemoveIfExists(testMovieNight.MovieNightStartHangfireId);
                await this.mediator.Send(new GuildMovieNights.Delete(testMovieNight));
            }

            private static readonly IEnumerable<Tuple<string, string, OmdbParentalRating, int>> testMoviesIdTitleAndRating = new List<Tuple<string, string, OmdbParentalRating, int>>()
            {
                new Tuple<string, string, OmdbParentalRating, int>("tt0119303", "Home Alone 3", OmdbParentalRating.PG, 1997),
                new Tuple<string, string, OmdbParentalRating, int>("tt0104431", "Home Alone 2: Lost in New York", OmdbParentalRating.PG, 1992),
                new Tuple<string, string, OmdbParentalRating, int>("tt0099785", "Home Alone", OmdbParentalRating.PG, 1990),
                new Tuple<string, string, OmdbParentalRating, int>("tt0117998", "Twister", OmdbParentalRating.PG_13, 1996),
                new Tuple<string, string, OmdbParentalRating, int>("tt0173052", "The Prince and the Surfer", OmdbParentalRating.PG, 1999),
            };

            public async Task<IEnumerable<GuildMovieSuggestion>> AddTestMovies(DiscordUser suggestor, DiscordGuild guild)
            {
                List<GuildMovieSuggestion> testSuggestions = new();
                foreach (var value in testMoviesIdTitleAndRating)
                {
                    DbResult<GuildMovieSuggestion> getResult = await this.mediator.Send(new GuildMovieSuggestions.GetMovieSuggestion(value.Item1, guild.Id));
                    if (!getResult.TryGetValue(out GuildMovieSuggestion? gms))
                    {
                        DbResult<GuildMovieSuggestion> addResult = await this.mediator.Send(new GuildMovieSuggestions.Add(value.Item1, suggestor, value.Item2, guild, value.Item4, value.Item3));
                        if (!addResult.TryGetValue(out GuildMovieSuggestion? gms2))
                            throw new Exception("Error occurred while adding movie suggestions");
                        testSuggestions.Add(gms2);
                    }
                }

                return testSuggestions;
            }

            private const string TEST_MOVIE_NIGHT_VOTING_START_ID = "test-movie-night-voting-start-id-";
            private const string TEST_MOVIE_NIGHT_VOTING_END_ID = "test-movie-night-voting-end-id-";
            private const string TEST_MOVIE_NIGHT_MOVIE_NIGHT_START_ID = "test-movie-night-movie-night-start-id-";
            public async Task<GuildMovieNight> AddTestMovieNight(DiscordGuild guild, DiscordChannel channel, DiscordUser user)
            {
                string guid = Guid.NewGuid().ToString();
                RecurringJob.AddOrUpdate<TestService>(TEST_MOVIE_NIGHT_VOTING_START_ID + guid, s => s.LogTestMessage(TEST_MOVIE_NIGHT_VOTING_START_ID), Cron.Minutely());
                RecurringJob.AddOrUpdate<TestService>(TEST_MOVIE_NIGHT_VOTING_END_ID + guid, s => s.LogTestMessage(TEST_MOVIE_NIGHT_VOTING_END_ID), Cron.Minutely());
                RecurringJob.AddOrUpdate<TestService>(TEST_MOVIE_NIGHT_MOVIE_NIGHT_START_ID + guid, s => s.LogTestMessage(TEST_MOVIE_NIGHT_MOVIE_NIGHT_START_ID), Cron.Minutely());
                DbResult<GuildMovieNight> addResult = await this.mediator.Send(
                    new GuildMovieNights.Add(
                        TEST_MOVIE_NIGHT_VOTING_START_ID,
                        TEST_MOVIE_NIGHT_VOTING_END_ID,
                        TEST_MOVIE_NIGHT_MOVIE_NIGHT_START_ID,
                        4,
                        OmdbParentalRating.PG,
                        guild,
                        channel,
                        user)
                    );
                
                if (addResult.TryGetValue(out GuildMovieNight? movieNight))
                {
                    return movieNight;
                }

                throw new Exception("An error occurred in adding the test movie night");
            }
        }
    }
}
