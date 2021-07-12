using CronEspresso.NETCore;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Hangfire;
using Hangfire.Storage;
using MediatR;
using Norm.Database.Entities;
using Norm.Database.Requests;
using Norm.Modules.Exceptions;
using Norm.Omdb;
using Norm.Omdb.Enums;
using Norm.Omdb.Types;
using Norm.Services;
using Norm.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Norm.Modules
{
    [Group("movie_night")]
    [Aliases("mn")]
    [Description("The group for creating movie nights, making movie suggestions, and deleting movie nights")]
    public class MovieNightModule : BaseCommandModule
    {
        private readonly OmdbClient omdbClient;
        private readonly IMediator mediator;
        private readonly BotService bot;
        private readonly NodaTimeConverterService nodaTimeConverterService;

        public MovieNightModule(OmdbClient omdbClient, IMediator mediator, BotService bot, NodaTimeConverterService nodaTimeConverterService)
        {
            this.omdbClient = omdbClient;
            this.mediator = mediator;
            this.bot = bot;
            this.nodaTimeConverterService = nodaTimeConverterService;
        }

        [Command("suggest")]
        [Aliases("s")]
        [Description("Start the interactive search process with the query you provide.")]
        public async Task SuggestAsync(CommandContext context, [RemainingText, Description("The movie to search for")] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                await context.RespondAsync("You must provide a non-empty query");
                return;
            }

            LazyOmdbList movieList;
            try
            {
                movieList = await this.omdbClient.SearchByTitleAsync(query);
            }
            catch (OmdbException oe)
            {
                await context.RespondAsync($"There was some failure with your search: {oe.Message}");
                return;
            }

            OmdbMovie? selectedMovie = await SelectMovieWithPaginatedEmbed(movieList, context);
            if (selectedMovie == null)
                return;

            DbResult getGuildMovieSuggestionResult = await this.mediator.Send(new GuildMovieSuggestions.GetMovieSuggestion(selectedMovie.ImdbId, context.Guild));

            if (getGuildMovieSuggestionResult.Success)
            {
                await context.RespondAsync("This movie suggestion has already been made");
                return;
            }

            DbResult<GuildMovieSuggestion> addMovieSuggestionResult = await this.mediator.Send(new GuildMovieSuggestions.Add(selectedMovie.ImdbId, context.Member, selectedMovie.Title, context.Guild, selectedMovie.Year, selectedMovie.Rated ?? OmdbParentalRating.NR));
            if (!addMovieSuggestionResult.TryGetValue(out GuildMovieSuggestion? _))
            {
                await context.RespondAsync("There was a failure adding your movie suggestion to the data store. Reach out to your developer.");
                return;
            }

            await context.RespondAsync($"You have added {selectedMovie.Title} as a suggestion!");
        }

        private async Task<OmdbMovie?> SelectMovieWithPaginatedEmbed(LazyOmdbList movieList, CommandContext context)
        {
            List<(OmdbMovie, DiscordEmbedBuilder)> omdbMovies = new();
            int currentIndex = 0;

            await context.RespondAsync(SELECT_MOVIE_TEXT);
            DiscordMessage msg = await context.Channel.SendMessageAsync($"{DiscordEmoji.FromGuildEmote(context.Client, 848012958851399710)} {Formatter.Bold(context.Guild.CurrentMember.DisplayName)} is getting your search results");
            _ = Task.Run(AddPaginationEmojis(msg));

            InteractivityExtension interactivity = context.Client.GetInteractivity();
            bool selected = false;
            do
            {
                if (currentIndex == omdbMovies.Count)
                {
                    try
                    {
                        OmdbMovie newMovie = await this.omdbClient.GetByImdbIdAsync(movieList.CurrentItem().ImdbId, omdbPlotOption: OmdbPlotOption.SHORT);
                        omdbMovies.Add((newMovie, newMovie.ToDiscordEmbedBuilder()));
                    }
                    catch (JsonException e)
                    {
                        await this.bot.LogExceptions(e);
                        if (movieList.HasNext())
                        {
                            await movieList.MoveNext();
                            continue;
                        }
                        else
                        {
                            currentIndex -= 1;
                        }
                    }
                }

                await msg.ModifyAsync(content: string.Empty, embed: omdbMovies[currentIndex].Item2.Build());

                InteractivityResult<MessageReactionAddEventArgs> waitForReactionResult = await interactivity.WaitForReactionAsync(this.ReactionIsPaginationEmoji, msg, context.Member);
                if (waitForReactionResult.TimedOut)
                {
                    await context.RespondAsync("You idled for too long and this search has been cancelled.");
                    return null;
                }

                await waitForReactionResult.Result.Message.DeleteReactionAsync(waitForReactionResult.Result.Emoji, context.Member);

                switch (waitForReactionResult.Result.Emoji.Name)
                {
                    case "⏪":
                        currentIndex = 0;
                        break;
                    case "⬅️":
                        currentIndex = await MoveLeft(movieList, currentIndex);
                        break;
                    case "✅":
                        selected = true;
                        break;
                    case "➡️":
                        currentIndex = await MoveRight(movieList, currentIndex);
                        break;
                    case "⏹️":
                        await context.RespondAsync("Stopping the search.");
                        return null;
                    default:
                        await SendInvalidSelectionMessage(context);
                        break;
                }
            }
            while (!selected);

            return omdbMovies[currentIndex].Item1;
        }

        private const string SELECT_MOVIE_TEXT =
@"Select a movie by reacting with the :white_check_mark:
Go back to the beginning of your results by reacting with the :rewind:
Go back to the previous result by reacting with the :arrow_left:
Go to the next result by reacting with the :arrow_right:
Cancel the search by reacting with the :stop_button:";

        private static async Task<int> MoveLeft(LazyOmdbList movieList, int currentIndex)
        {
            if (movieList.HasPrev())
            {
                currentIndex -= 1;
                await movieList.MovePrev();
            }

            return currentIndex;
        }

        private static async Task<int> MoveRight(LazyOmdbList movieList, int currentIndex)
        {
            if (movieList.HasNext())
            {
                currentIndex += 1;
                await movieList.MoveNext();
            }

            return currentIndex;
        }

        private static async Task SendInvalidSelectionMessage(CommandContext context)
        {
            DiscordMessage invalidMsg = await context.RespondAsync("Invalid selection");
            _ = Task.Run(async () =>
            {
                await Task.Delay(3000);
                await invalidMsg.DeleteAsync();
            });
        }

        private static Func<Task> AddPaginationEmojis(DiscordMessage msg)
        {
            return async () =>
            {
                foreach (string emojiName in paginationEmojiNames)
                {
                    await msg.CreateReactionAsync(DiscordEmoji.FromUnicode(emojiName));
                }
            };
        }

        private static readonly string[] paginationEmojiNames = new string[] { "⏪", "⬅️", "✅", "➡️", "⏹️" };

        private bool ReactionIsPaginationEmoji(MessageReactionAddEventArgs eventArgs)
        {
            return paginationEmojiNames.Contains(eventArgs.Emoji.Name);
        }

        [Command("unsuggest")]
        [Aliases("u")]
        [Description("Start the interactive movie suggestion deletion process")]
        public async Task UnsuggestAsync(CommandContext context)
        {
            DbResult<IEnumerable<GuildMovieSuggestion>> suggestionsResult = await GetSuggestionsResult(context);
            if (!suggestionsResult.TryGetValue(out IEnumerable<GuildMovieSuggestion>? result))
            {
                await context.RespondAsync("Something went wrong while attempting to get the suggestions. Please contact the developer.");
                return;
            }

            if (!result.Any())
            {
                await context.RespondAsync("You do not have any suggestions to delete.");
                return;
            }

            List<GuildMovieSuggestion> suggestions = result.ToList();
            InteractivityExtension interactivity = context.Client.GetInteractivity();
            IEnumerable<Page> pages = GetGuildMovieSuggestionsPages(suggestions.ToList(), interactivity);

            CustomResult<int> waitResult = await context.WaitForMessageAndPaginateOnMsg(pages,
                PaginationMessageFunction.CreateWaitForMessageWithIntInRange(context.User, context.Channel, 1, suggestions.Count + 1)
            );

            if (waitResult.Cancelled)
            {
                await context.RespondAsync("Ok, I won't delete any suggestion. Please try again if so desired.");
                return;
            }

            if (waitResult.TimedOut)
            {
                await context.RespondAsync("You never gave me a valid input. Please try again if so desired.");
                return;
            }

            Reaction reaction = await interactivity.AddAndWaitForYesNoReaction(
                await context.Channel.SendMessageAsync($"You want me to do delete the suggestion `{suggestions[waitResult.Result - 1].Title}`?"), 
                context.Member
            );

            if (reaction != Reaction.Yes)
            {
                await context.Channel.SendMessageAsync("Ok!");
                return;
            }

            GuildMovieSuggestion chosen = suggestions[waitResult.Result - 1];
            await this.mediator.Send(new GuildMovieSuggestions.Delete(chosen));
            await context.Channel.SendMessageAsync($"{context.Member.Mention}, I have deleted the suggestion `{suggestions[waitResult.Result - 1].Title}`");
        }

        private async Task<DbResult<IEnumerable<GuildMovieSuggestion>>> GetSuggestionsResult(CommandContext context)
        {
            DbResult<IEnumerable<GuildMovieSuggestion>> suggestionsResult;
            if (context.Member.PermissionsIn(context.Channel).HasPermission(Permissions.ManageGuild))
            {
                suggestionsResult = await this.mediator.Send(new GuildMovieSuggestions.GetGuildMovieSuggestions(context.Guild));
            }
            else
            {
                suggestionsResult = await this.mediator.Send(new GuildMovieSuggestions.GetUsersGuildMovieSuggestions(context.Guild, context.Member));
            }

            return suggestionsResult;
        }

        private static IEnumerable<Page> GetGuildMovieSuggestionsPages(List<GuildMovieSuggestion> suggestions, InteractivityExtension interactivity)
        {
            StringBuilder builder = new StringBuilder();
            int count = 1;
            foreach (GuildMovieSuggestion suggestion in suggestions)
            {
                builder.AppendLine($"{count}. {suggestion.Title}");
                count += 1;
            }

            DiscordEmbedBuilder embedBase = new DiscordEmbedBuilder().WithTitle("Select a movie night by typing: <number>");
            return interactivity.GeneratePagesInEmbed(builder.ToString(), DSharpPlus.Interactivity.Enums.SplitType.Line, embedBase);
        }

        [Command("create")]
        [Aliases("c")]
        [RequireUserPermissions(Permissions.MentionEveryone)]
        [Description("Start the interactive movie night creation process for the movie night that will be announced in the announcement channel you specify.")]
        public async Task CreateAsync(CommandContext context, [Description("The channel in which to announce the movie night")] DiscordChannel announcementChannel)
        {
            TimeZoneInfo hostTimeZoneInfo = await GetUserTimeZoneInfoAsync(context);

            DiscordMessage confirmationMessage = await context.RespondAsync("Hi, you'd like to schedule a recurring movie night?");
            InteractivityExtension interactivity = context.Client.GetInteractivity();
            Reaction reaction = await interactivity.AddAndWaitForYesNoReaction(confirmationMessage, context.Member);

            if (reaction != Reaction.Yes)
            {
                await context.Channel.SendMessageAsync("Ok. I will end the process.");
                return;
            }

            DayOfWeek movieStartDayOfWeek = await GetMovieStartDayOfWeek(context, interactivity);
            TimeSpan movieStartTimeOfDay = await GetMovieStartTimeOfDay(context, interactivity);

            await context.Channel.SendMessageAsync("How many days and hours before the movie starts do you want voting to end? Format: 0d0h");
            (int voteEndDays, int voteEndHours) = await GetDaysAndHours(context, interactivity);
            (DayOfWeek voteEndDayOfWeek, TimeSpan voteEndTimeSpan) = GenerateCronEspressoVariables(movieStartDayOfWeek, movieStartTimeOfDay, voteEndDays, voteEndHours);

            await context.Channel.SendMessageAsync("How many days and hours before the movie starts do you want voting to start? Format: 0d0h");
            (int voteStartDays, int voteStartHours) = await GetDaysAndHours(context, interactivity);
            (DayOfWeek voteStartDayOfWeek, TimeSpan voteStartTimeSpan) = GenerateCronEspressoVariables(movieStartDayOfWeek, movieStartTimeOfDay, voteStartDays, voteStartHours);

            OmdbParentalRating maxParentalRating = await GetMaxParentalRating(context, interactivity);

            int maximumNumberOfSuggestions = await GetMaximumNumberOfSuggestions(context, interactivity);

            string voteStartCron = GenerateCronExpression(voteStartTimeSpan, voteStartDayOfWeek);
            string voteEndCron = GenerateCronExpression(voteEndTimeSpan, voteEndDayOfWeek);
            string movieStartCron = GenerateCronExpression(movieStartTimeOfDay, movieStartDayOfWeek);

            string guid = Guid.NewGuid().ToString();
            string voteStartJobId = $"{context.Guild.Id}-{context.Member.Id}-{guid}-voting-start";
            string voteEndJobId = $"{context.Guild.Id}-{context.Member.Id}-{guid}-voting-end";
            string startMovieJobId = $"{context.Guild.Id}-{context.Member.Id}-{guid}-start-movie";

            DbResult<GuildMovieNight> addMovieNightResult = await this.mediator.Send(new GuildMovieNights.Add(voteStartJobId, voteEndJobId, startMovieJobId, maximumNumberOfSuggestions, maxParentalRating, context.Guild, announcementChannel, context.Member));

            if (!addMovieNightResult.TryGetValue(out GuildMovieNight? movieNight))
            {
                await context.Channel.SendMessageAsync("I failed in adding the movie night to the database. Logs have been sent to the developer.");
                throw new Exception(
$@"voteStartJobId: {voteStartJobId}
voteEndJobId: {voteEndJobId}
startMovieJobId: {startMovieJobId}
maximumNumberOfSuggestions: {maximumNumberOfSuggestions}
maxParentalRating: {maxParentalRating}
guild: {context.Guild.Id}
announcementChannel: {announcementChannel.Id}
host: {context.Member.Id}");
            }

            RecurringJob.AddOrUpdate<MovieNightService>(voteStartJobId, mns => mns.StartVoting(movieNight.Id), voteStartCron, new RecurringJobOptions() { TimeZone = hostTimeZoneInfo });
            RecurringJob.AddOrUpdate<MovieNightService>(voteEndJobId, mns => mns.CalculateVotes(movieNight.Id), voteEndCron, new RecurringJobOptions() { TimeZone = hostTimeZoneInfo });
            RecurringJob.AddOrUpdate<MovieNightService>(startMovieJobId, mns => mns.StartMovie(movieNight.Id), movieStartCron, new RecurringJobOptions() { TimeZone = hostTimeZoneInfo });
            Dictionary<string, RecurringJobDto>? rJobDtos = JobStorage.Current
                .GetConnection()
                .GetRecurringJobs(new List<string>() { voteStartJobId, voteEndJobId, startMovieJobId })
                .ToDictionary(x => x.Id);

            await context.Channel.SendMessageAsync("Your movie night has been scheduled.");
            if (rJobDtos[voteStartJobId].NextExecution!.Value > rJobDtos[startMovieJobId].NextExecution!.Value)
            {
                await context.Channel.SendMessageAsync($"{context.Member.Mention}, the next scheduled voting will happen after the movie night is supposed to happen. To handle this, we are going to open voting now and close voting at the normal scheduled time. If the normal scheduled time to close voting can't be used, we will cancel the next movie night altogether and will not open voting.");

                if (rJobDtos[voteEndJobId].NextExecution!.Value > rJobDtos[startMovieJobId].NextExecution!.Value)
                {
                    JobStorage.Current.GetConnection().SetRangeInHash(
                        $"recurring-job:{startMovieJobId}",
                        new[] { new KeyValuePair<string, string>("skip", "true") }
                    );
                }

                BackgroundJob.Enqueue<MovieNightService>(mns => mns.StartVoting(movieNight.Id));
            }
        }

        private static async Task<int> GetMaximumNumberOfSuggestions(CommandContext context, InteractivityExtension interactivity)
        {
            await context.Channel.SendMessageAsync("How many suggestions do you want pulled to vote on? (3-10)");
            int? maximumNumberOfSuggestions = null;
            while (maximumNumberOfSuggestions == null)
            {
                InteractivityResult<DiscordMessage> getMaxNumSuggestions = await interactivity.WaitForMessageAsync(m => m.ChannelId == context.Channel.Id && m.Author.Id == context.Member.Id);
                if (getMaxNumSuggestions.TimedOut)
                {
                    throw new UserTimeoutException("movie night creation");
                }

                if (int.TryParse(getMaxNumSuggestions.Result.Content, out int maxNumSuggestions) && maxNumSuggestions <= 10 && maxNumSuggestions >= 3)
                {
                    maximumNumberOfSuggestions = maxNumSuggestions;
                }
                else
                {
                    await context.Channel.SendMessageAsync("Invalid Input. Please try again.");
                }
            }

            return maximumNumberOfSuggestions.Value;
        }

        private static async Task<OmdbParentalRating> GetMaxParentalRating(CommandContext context, InteractivityExtension interactivity)
        {
            await context.Channel.SendMessageAsync("What is the maximum parental rating you want included in the movie night?");
            OmdbParentalRating? maxParentalRating = null;
            while (maxParentalRating == null)
            {
                InteractivityResult<DiscordMessage> getParentalRating = await interactivity.WaitForMessageAsync(m => m.ChannelId == context.Channel.Id && m.Author.Id == context.Member.Id);
                if (getParentalRating.TimedOut)
                {
                    throw new UserTimeoutException("movie night creation");
                }

                try { maxParentalRating = getParentalRating.Result.Content.ToOmdbParentalRating(); } catch (JsonException) { }

                if (maxParentalRating == null)
                {
                    await context.Channel.SendMessageAsync("Invalid Input. Please try again with an MPA Parental Rating. If you are sure that the rating exists, please contact the developer.");
                }
            }

            return maxParentalRating.Value;
        }

        private static async Task<(int, int)> GetDaysAndHours(CommandContext context, InteractivityExtension interactivity)
        {
            int? voteEndDays = null;
            int? voteEndHours = null;
            while (voteEndDays == null || voteEndHours == null)
            {
                InteractivityResult<DiscordMessage> getVoteEnd = await interactivity.WaitForMessageAsync(m => m.ChannelId == context.Channel.Id && m.Author.Id == context.Member.Id);
                if (getVoteEnd.TimedOut)
                {
                    throw new UserTimeoutException("movie night creation");
                }

                if (!TimeSpan.TryParseExact(getVoteEnd.Result.Content, @"d\d%h\h", null, out TimeSpan parsedTimeSpan))
                {
                    await context.Channel.SendMessageAsync("Invalid input. Please try again.");
                }
                else if (parsedTimeSpan.TotalDays >= 0 && parsedTimeSpan.TotalDays < 7)
                {
                    voteEndDays = parsedTimeSpan.Days;
                    voteEndHours = parsedTimeSpan.Hours;
                }
                else
                {
                    await context.Channel.SendMessageAsync("You must provide a time within 0d0h-6d23h.");
                }
            }

            return (voteEndDays.Value, voteEndHours.Value);
        }

        private static async Task<TimeSpan> GetMovieStartTimeOfDay(CommandContext context, InteractivityExtension interactivity)
        {
            await context.Channel.SendMessageAsync("What time would you like to show the movie? (respond with the time in 24h format. Ex: 13:20 for 1:20 pm)");
            TimeSpan? movieStartTimeOfDay = null;
            while (movieStartTimeOfDay == null)
            {
                InteractivityResult<DiscordMessage> getTimeResult = await interactivity.WaitForMessageAsync(m => m.ChannelId == context.Channel.Id && m.Author.Id == context.Member.Id);
                if (getTimeResult.TimedOut)
                {
                    throw new UserTimeoutException("movie night creation");
                }

                if (!TimeSpan.TryParseExact(getTimeResult.Result.Content, @"hh\:mm", null, out TimeSpan parsedTimeSpan))
                {
                    await context.Channel.SendMessageAsync("Invalid input. Please try again.");
                }
                else if (parsedTimeSpan.TotalHours >= 0 && parsedTimeSpan.TotalHours < 24)
                {
                    movieStartTimeOfDay = parsedTimeSpan;
                }
                else
                {
                    await context.Channel.SendMessageAsync("You must provide a time within 00:00-23:59.");
                }
            }

            return movieStartTimeOfDay.Value;
        }

        private static async Task<DayOfWeek> GetMovieStartDayOfWeek(CommandContext context, InteractivityExtension interactivity)
        {
            await context.Channel.SendMessageAsync("What day of the week would you like to show the movie? (respond with the full day name or the three letter abbreviation)");
            DayOfWeek? movieStartDayOfWeek = null;
            while (movieStartDayOfWeek == null)
            {
                InteractivityResult<DiscordMessage> getDayOfWeekResult = await interactivity.WaitForMessageAsync(m => m.ChannelId == context.Channel.Id && m.Author.Id == context.Member.Id);
                if (getDayOfWeekResult.TimedOut)
                {
                    throw new UserTimeoutException("movie night creation");
                }

                movieStartDayOfWeek = ParseDayOfWeek(getDayOfWeekResult.Result.Content);

                if (movieStartDayOfWeek == null)
                {
                    await context.Channel.SendMessageAsync("Invalid input. Please try again.");
                }
            }

            return movieStartDayOfWeek.Value;
        }

        private async Task<TimeZoneInfo> GetUserTimeZoneInfoAsync(CommandContext context)
        {
            DbResult<UserTimeZone> getUserTimeZoneResult = await this.mediator.Send(new UserTimeZones.GetUsersTimeZone(context.Member));

            if (!getUserTimeZoneResult.TryGetValue(out UserTimeZone? userTimeZone))
            {
                throw new TimezoneNotSetupException();
            }

            return this.nodaTimeConverterService.ConvertToTimeZoneInfo(userTimeZone.TimeZoneId);
        }

        [Command("delete")]
        [Aliases("d")]
        [Description("Start the interactive movie night deletion process")]
        [RequirePermissions(Permissions.MentionEveryone)]
        public async Task DeleteAsync(CommandContext context)
        {
            DbResult<IEnumerable<GuildMovieNight>> getMovieNightsResult = await this.mediator.Send(new GuildMovieNights.GetAllGuildsMovieNights(context.Guild.Id));
            if (!getMovieNightsResult.TryGetValue(out IEnumerable<GuildMovieNight>? guildMovieNights))
            {
                throw new Exception("An error occured while retrieving guild movie nights");
            }
            bool hasManageServer = context.Member.Roles.Select(x => x.CheckPermission(Permissions.ManageGuild)).Any();
            if (!hasManageServer)
            {
                guildMovieNights = guildMovieNights.Where(mn => mn.HostId == context.Member.Id);
            }
            List<GuildMovieNight> movieNights = guildMovieNights.ToList();
            InteractivityExtension interactivity = context.Client.GetInteractivity();
            IEnumerable<Page> pages = await GetGuildMovieNightsPages(context.Guild, movieNights, interactivity, hasManageServer);
            CustomResult<int> result = await context.WaitForMessageAndPaginateOnMsg(pages,
                PaginationMessageFunction.CreateWaitForMessageWithIntInRange(context.User, context.Channel, 1, movieNights.Count + 1)
            );
            if (result.TimedOut || result.Cancelled)
            {
                await context.RespondAsync("You never gave me a valid input. Please try again if so desired.");
                return;
            }

            Reaction reaction = await interactivity.AddAndWaitForYesNoReaction(await context.Channel.SendMessageAsync($"You want me to do delete movie night {result.Result}?"), context.Member);

            if (reaction != Reaction.Yes)
            {
                await context.Channel.SendMessageAsync("Ok!");
                return;
            }

            GuildMovieNight chosen = movieNights[result.Result - 1];
            RecurringJob.RemoveIfExists(chosen.MovieNightStartHangfireId);
            RecurringJob.RemoveIfExists(chosen.VotingStartHangfireId);
            RecurringJob.RemoveIfExists(chosen.VotingEndHangfireId);
            await this.mediator.Send(new GuildMovieNights.Delete(chosen));
            await context.Channel.SendMessageAsync($"{context.Member.Mention}, I have deleted movie night {result.Result}");
        }

        private async static Task<IEnumerable<Page>> GetGuildMovieNightsPages(DiscordGuild guild, List<GuildMovieNight> movieNights, InteractivityExtension interactivity, bool hasManageServer)
        {
            StringBuilder guildMovieNightStringBuilder = new();

            int count = 1;
            HashSet<string> movieNightIds = movieNights.Select(mn => mn.MovieNightStartHangfireId).ToHashSet();
            Dictionary<string, RecurringJobDto> rJobDict = JobStorage
                .Current.GetConnection()
                .GetRecurringJobs()
                .Where(job => movieNightIds.Contains(job.Id))
                .ToDictionary(rjobDto => rjobDto.Id);

            if (movieNights.Count == rJobDict.Count)
            {
                foreach (GuildMovieNight movieNight in movieNights)
                {
                    string cron = rJobDict[movieNight.MovieNightStartHangfireId].Cron;
                    cron = CronExpressionDescriptor.ExpressionDescriptor.GetDescription(cron);
                    guildMovieNightStringBuilder.AppendLine($"{count}. {(hasManageServer ? $"{(await guild.GetMemberAsync(movieNight.HostId)).DisplayName}: " : "")}{cron}");
                    count += 1;
                }
            }

            if (!movieNights.Any() || !rJobDict.Any())
            {
                guildMovieNightStringBuilder.AppendLine("This guild does not have any movie nights currently running");
            }

            DiscordEmbedBuilder embedBase = new DiscordEmbedBuilder().WithTitle("Select a movie night by typing: <number>");
            return interactivity.GeneratePagesInEmbed(guildMovieNightStringBuilder.ToString(), DSharpPlus.Interactivity.Enums.SplitType.Line, embedBase);
        }

        private static string GenerateCronExpression(TimeSpan voteStartTimeSpan, DayOfWeek voteStartDayOfWeek)
        {
            string expression = CronGenerator.GenerateSetDayCronExpression(voteStartTimeSpan, voteStartDayOfWeek).Trim();
            // The expression produced is nearly correct but it's designed for Quartz not standard Cron and takes a nonstandard 7th field so 
            // we have to remove that last field.
            return expression.Substring(0, expression.LastIndexOf(' '));
        }

        private static (DayOfWeek, TimeSpan) GenerateCronEspressoVariables(DayOfWeek movieStartDayOfWeek, TimeSpan movieStartTimeOfDay, int numDaysBack, int numHoursBack)
        {
            int hourTime = movieStartTimeOfDay.Hours - numHoursBack;
            if (hourTime < 0)
            {
                numDaysBack += 1;
                hourTime.Modulo(24);
            }

            TimeSpan newTS = new(hourTime, movieStartTimeOfDay.Minutes, 0);
            DayOfWeek newDoW = (DayOfWeek)((int)movieStartDayOfWeek - numDaysBack).Modulo(7);

            return (newDoW, newTS);
        }

        private static DayOfWeek? ParseDayOfWeek(string content)
        {
            return content.Trim().ToLower() switch
            {
                "sun" or "sunday" => DayOfWeek.Sunday,
                "m" or "mon" or "monday" => DayOfWeek.Monday,
                "t" or "tue" or "tuesday" => DayOfWeek.Tuesday,
                "w" or "wed" or "wednesday" => DayOfWeek.Wednesday,
                "th" or "thu" or "thursday" => DayOfWeek.Thursday,
                "f" or "fri" or "friday" => DayOfWeek.Friday,
                "s" or "sat" or "saturday" => DayOfWeek.Saturday,
                _ => null
            };
        }
    }
}
