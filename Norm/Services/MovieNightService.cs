using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.EventHandling;
using DSharpPlus.Interactivity.Extensions;
using Hangfire;
using Hangfire.Storage;
using MediatR;
using NodaTime;
using Norm.Attributes;
using Norm.Database.Entities;
using Norm.Database.Requests;
using Norm.Omdb;
using Norm.Omdb.Enums;
using Norm.Omdb.Types;
using Norm.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Norm.Services
{
    public class MovieNightService
    {
        private readonly BotService bot;
        private readonly IMediator mediator;
        private readonly IDateTimeZoneProvider timeZoneProvider;
        private readonly OmdbClient omdbClient;
        private readonly IClock clock;

        public MovieNightService(BotService bot, IMediator mediator, IDateTimeZoneProvider timeZoneProvider, OmdbClient omdbClient, IClock clock)
        {
            this.bot = bot;
            this.mediator = mediator;
            this.timeZoneProvider = timeZoneProvider;
            this.omdbClient = omdbClient;
            this.clock = clock;
        }

        /// <summary>
        /// Generate the embed with the randomly selected movies and add emojis to allow for voting
        /// </summary>
        /// <param name="movieNightId">ID for the movie night in the data store</param>
        /// <exception cref="ArgumentException">Thrown when an unknown movie night ID is provided</exception>
        public async Task StartVoting(int movieNightId)
        {
            GuildMovieNight movieNight = await GetGuildMovieNightAsync(movieNightId);

            (DiscordClient client, DiscordGuild guild, DiscordChannel channel) = await this.GetCommonDiscordObjects(movieNight);

            DbResult<IEnumerable<GuildMovieSuggestion>> randomSuggestionsResult = await this
                .mediator.Send(new GuildMovieSuggestions.GetRandomGuildMovieSuggestions(guild, movieNight.NumberOfSuggestions, movieNight.MaximumRating));

            if (!randomSuggestionsResult.TryGetValue(out IEnumerable<GuildMovieSuggestion>? randomSuggestions))
                throw new Exception("Something went wrong with getting the random suggestions.");

            string description = AddMovieSuggestionsAndGenerateDescription(client, movieNight, randomSuggestions);
            RecurringJobDto rJobDto = GetMovieNightStartRecurringJobInfo(movieNight);

            DateTimeZone hostDTZ = await GetUserDateTimeZone(movieNight.HostId);
            ZonedDateTime zdt = GetJobsZonedDateTime(rJobDto, hostDTZ);

            DiscordEmbed eBuilder = new DiscordEmbedBuilder()
                .WithTitle($"Time to vote for a movie!")
                .WithDescription(description)
                .AddField("Date and Time of Movie", zdt.ToString("MM/dd/yyyy hh:mm x", null), true)
                .AddField("Maximum Parental Rating", movieNight.MaximumRating.ToQueryValue(), true);

            DiscordMessageBuilder mBuilder = new DiscordMessageBuilder()
                .WithContent("@everyone")
                .WithEmbed(eBuilder);

            DiscordMessage votingMessage = await channel.SendMessageAsync(mBuilder);
            movieNight.VotingMessageId = votingMessage.Id;

            await this.mediator.Send(new GuildMovieNights.Update(movieNight));
            foreach (DiscordEmoji emoji in GetNumberEmojis(client).Take(randomSuggestions.Count()))
            {
                await votingMessage.CreateReactionAsync(emoji);
            }
        }

        /// <summary>
        /// Determine the number of votes that each movie got and then select the highest ranked movie.
        /// If there is a tie on more than one of the movies, message the movie night creator with an
        /// embed where they will break the tie.
        /// </summary>
        /// <param name="movieNightId">ID for the movie night in the data store</param>
        public async Task CalculateVotes(int movieNightId)
        {
            GuildMovieNight movieNight = await GetGuildMovieNightAsync(movieNightId);

            (DiscordClient client, DiscordGuild guild, DiscordChannel channel) = await this.GetCommonDiscordObjects(movieNight);
            DiscordMessage votingMessage = await channel.GetMessageAsync(movieNight.VotingMessageId ?? throw new Exception("Somehow, some way, the voting message id was null... something done f$*@ed up."));
            Dictionary<string, DiscordReaction> mostReactedReactions = GetMostReactedReactons(votingMessage);

            DiscordMember host = await guild.GetMemberAsync(movieNight.HostId);
            GuildMovieSuggestion winningSuggestion = await GetWinningSuggestion(client, guild, host, movieNight, mostReactedReactions);

            movieNight.WinningMovieImdbId = winningSuggestion.ImdbId;
            DbResult movieNightUpdateResult = await this.mediator.Send(new GuildMovieNights.Update(movieNight));
            if (!movieNightUpdateResult.Success)
            {
                throw new Exception("An error occurred in updating the movie night with the winning suggestion");
            }

            RecurringJobDto rJobDto = GetMovieNightStartRecurringJobInfo(movieNight);

            LocalDateTime ldt = LocalDateTime.FromDateTime(rJobDto.NextExecution!.Value);
            DateTimeZone hostDTZ = await GetUserDateTimeZone(movieNight.HostId);

            ZonedDateTime zdt = ldt.InUtc();
            zdt = zdt.WithZone(hostDTZ);

            OmdbMovie movieInfo = await this.omdbClient.GetByImdbIdAsync(winningSuggestion.ImdbId, omdbPlotOption: OmdbPlotOption.SHORT);

            DiscordEmbedBuilder announceWinnerEmbed = movieInfo.ToDiscordEmbedBuilder(true)
                .WithAuthor(host.DisplayName, iconUrl: host.AvatarUrl);
            DiscordMessageBuilder announceWinnerMessage = new DiscordMessageBuilder()
                .WithContent($"@everyone, here's what {host.Mention} is showing {zdt.ToString("MM/dd/yyyy hh:mm x", null)}")
                .WithEmbed(announceWinnerEmbed.Build());

            await channel.SendMessageAsync(announceWinnerMessage);
        }

        /// <summary>
        /// Make an announcement about the movie starting and then begin tracking who shows up over the
        /// next two hours to see if the people who voted showed up to the movie. If they didn't, make 
        /// them not able to vote for the next weeks movie.
        /// </summary>
        /// <param name="movieNightId">ID for the movie night in the data store</param>
        [SkipRunning]
        public async Task StartMovie(int movieNightId)
        {
            GuildMovieNight movieNight = await GetGuildMovieNightAsync(movieNightId);

            if (movieNight.WinningMovieImdbId == null)
            {
                throw new Exception("The Winning Movie IMDb ID was null at the point in time of the Start Movie.");
            }

            (DiscordClient _, DiscordGuild guild, DiscordChannel channel) = await this.GetCommonDiscordObjects(movieNight);
            DiscordMember host = await guild.GetMemberAsync(movieNight.HostId);

            OmdbMovie movieInfo = await this.omdbClient.GetByImdbIdAsync(movieNight.WinningMovieImdbId, omdbPlotOption: OmdbPlotOption.SHORT);
            DiscordEmbedBuilder announceWinnerEmbed = movieInfo.ToDiscordEmbedBuilder()
                .WithAuthor(host.DisplayName, iconUrl: host.AvatarUrl);
            DiscordMessageBuilder announceWinnerMessage = new DiscordMessageBuilder()
                .WithContent($"@everyone, the movie below is starting now!")
                .WithEmbed(announceWinnerEmbed.Build());

            await channel.SendMessageAsync(announceWinnerMessage);

            // Update movie suggestion with the time watched
            DbResult<GuildMovieSuggestion> getWinningMovieSuggestion = await this.mediator.Send(new GuildMovieSuggestions.GetMovieSuggestion(movieNight.WinningMovieImdbId, guild));
            if (!getWinningMovieSuggestion.TryGetValue(out GuildMovieSuggestion? winningMovieSuggestion))
            {
                return;
            }

            winningMovieSuggestion.InstantWatched = this.clock.GetCurrentInstant();
            await this.mediator.Send(new GuildMovieSuggestions.Update(winningMovieSuggestion));
        }

        // Private Async Methods
        private async Task<GuildMovieSuggestion> DoTiebreaker(DiscordClient client, DiscordGuild guild, DiscordMember host, GuildMovieNight movieNight, Dictionary<string, DiscordReaction> mostReactedReactions)
        {
            StringBuilder descriptionBuilder = new();
            var tiedSuggestions = movieNight.MovieNightAndSuggestions.Where(mns => mostReactedReactions.ContainsKey(mns.EmojiId)).Select(mns => mns.MovieSuggestion).ToList().Zip(GetNumberEmojis(client));

            DiscordEmbedBuilder tiebreakerEmbed = new DiscordEmbedBuilder().WithTitle("You need to break the tie, vote for your favorite of these options.");

            descriptionBuilder.AppendLine("If you don't respond within 10 minutes, a random movie will be selected");
            descriptionBuilder.AppendLine();
            foreach ((GuildMovieSuggestion gms, DiscordEmoji emoji) in tiedSuggestions)
            {
                descriptionBuilder.AppendLine($"{emoji}. {gms.Title}");
            }
            tiebreakerEmbed.WithDescription(descriptionBuilder.ToString());
            DiscordMessage tiebreakerMessage = await host.SendMessageAsync(tiebreakerEmbed);

            List<DiscordEmoji> emojis = tiedSuggestions.Select(x => x.Second).ToList();
            foreach (DiscordEmoji emoji in emojis)
            {
                await tiebreakerMessage.CreateReactionAsync(emoji);
            }

            HashSet<string> emojiNames = emojis.Select(x => x.Name).ToHashSet();
            InteractivityResult<MessageReactionAddEventArgs> tiebreakerReaction =
                await client
                .GetInteractivity()
                .WaitForReactionAsync(
                    x => x.Message.Id == tiebreakerMessage.Id &&
                         emojiNames.Contains(x.Emoji.Name),
                    host,
                    TimeSpan.FromMinutes(10)
                );

            if (tiebreakerReaction.TimedOut)
            {
                List<GuildMovieSuggestion> tiedList = tiedSuggestions.Select(x => x.First).ToList();
                return tiedList[new Random().Next(tiedList.Count)];
            }
            else
            {
                return tiedSuggestions.First(x => x.Second.Name == tiebreakerReaction.Result.Emoji.Name).First;
            }
        }
        private async Task<(DiscordClient client, DiscordGuild guild, DiscordChannel channel)> GetCommonDiscordObjects(GuildMovieNight movieNight)
        {
            DiscordClient client = this.bot.ShardedClient.GetShard(movieNight.GuildId);
            DiscordGuild guild = await client.GetGuildAsync(movieNight.GuildId);
            DiscordChannel channel = guild.GetChannel(movieNight.AnnouncementChannelId);
            return (client, guild, channel);
        }
        private async Task<GuildMovieNight> GetGuildMovieNightAsync(int movieNightId)
        {
            DbResult<GuildMovieNight> movieNightResult = await this.mediator.Send(new GuildMovieNights.GetMovieNight(movieNightId));
            if (!movieNightResult.TryGetValue(out GuildMovieNight? movieNight))
            {
                throw new ArgumentException("Unknown Movie Night ID ");
            }

            return movieNight;
        }
        private async Task<DateTimeZone> GetUserDateTimeZone(ulong hostId)
        {
            DbResult<UserTimeZone> hostTimeZoneResult = await this.mediator.Send(new UserTimeZones.GetUsersTimeZone(hostId));
            if (!hostTimeZoneResult.TryGetValue(out UserTimeZone? hostTimeZone))
            {
                throw new Exception("Host does not have their time zone set up");
            }
            DateTimeZone? hostDTZ = this.timeZoneProvider.GetZoneOrNull(hostTimeZone.TimeZoneId);
            if (hostDTZ == null)
            {
                throw new Exception("Unknown time zone id");
            }

            return hostDTZ;
        }
        private async Task<GuildMovieSuggestion> GetWinningSuggestion(DiscordClient client, DiscordGuild guild, DiscordMember host, GuildMovieNight movieNight, Dictionary<string, DiscordReaction> mostReactedReactions)
        {
            GuildMovieSuggestion winningSuggestion;
            if (mostReactedReactions.Count > 1)
            {
                winningSuggestion = await DoTiebreaker(client, guild, host, movieNight, mostReactedReactions);
            }
            else
            {
                winningSuggestion = movieNight.MovieNightAndSuggestions.First(mns => mns.EmojiId == mostReactedReactions.First().Value.Emoji.Name).MovieSuggestion;
            }

            return winningSuggestion;
        }

        // Private Static Methods
        private static string AddMovieSuggestionsAndGenerateDescription(DiscordClient client, GuildMovieNight movieNight, IEnumerable<GuildMovieSuggestion> suggestions)
        {
            if (GetNumberEmojis(client).Count() < suggestions.Count())
                throw new ArgumentException("Attempted to use more suggestions than is allowed by the system");

            StringBuilder descriptionBuilder = new();
            foreach ((GuildMovieSuggestion gms, DiscordEmoji emoji) in suggestions.Zip(GetNumberEmojis(client)))
            {
                movieNight.MovieNightAndSuggestions.Add(new MovieNightAndSuggestion(movieNight.Id, gms.ImdbId, emoji.Name, gms.GuildId));
                descriptionBuilder.AppendLine($"{emoji}. {gms.Title}");
            }

            return descriptionBuilder.ToString();
        }
        private static Dictionary<string, DiscordReaction> GetMostReactedReactons(DiscordMessage votingMessage)
        {
            Dictionary<string, DiscordReaction> mostReactedReactions = new() { { votingMessage.Reactions[0].Emoji.Name, votingMessage.Reactions[0] } };
            foreach (DiscordReaction reaction in votingMessage.Reactions.Skip(1))
            {
                int maxNumReacts = mostReactedReactions.First().Value.Count - (mostReactedReactions.First().Value.IsMe ? 1 : 0);
                int curNumReacts = reaction.Count - (reaction.IsMe ? 1 : 0);
                if (curNumReacts > maxNumReacts)
                {
                    mostReactedReactions = new() { { reaction.Emoji.Name, reaction } };
                }
                else if (curNumReacts == maxNumReacts)
                {
                    mostReactedReactions.Add(reaction.Emoji.Name, reaction);
                }
            }

            return mostReactedReactions;
        }

        private static IEnumerable<DiscordEmoji>? numbers = null;
        private static IEnumerable<DiscordEmoji> GetNumberEmojis(DiscordClient client)
        {
            return numbers ??= new List<DiscordEmoji>
            {
                DiscordEmoji.FromName(client, ":one:"),
                DiscordEmoji.FromName(client, ":two:"),
                DiscordEmoji.FromName(client, ":three:"),
                DiscordEmoji.FromName(client, ":four:"),
                DiscordEmoji.FromName(client, ":five:"),
                DiscordEmoji.FromName(client, ":six:"),
                DiscordEmoji.FromName(client, ":seven:"),
                DiscordEmoji.FromName(client, ":eight:"),
                DiscordEmoji.FromName(client, ":nine:"),
                DiscordEmoji.FromName(client, ":keycap_ten:"),
            };
        }
        private static RecurringJobDto GetMovieNightStartRecurringJobInfo(GuildMovieNight movieNight)
        {
            RecurringJobDto? rJobDto = JobStorage.Current.GetConnection().GetRecurringJobs().FirstOrDefault(x => x.Id == movieNight.MovieNightStartHangfireId);
            if (rJobDto == null || !rJobDto.NextExecution.HasValue)
            {
                throw new Exception("That Hangfire Job no longer exists or was never scheduled");
            }

            return rJobDto;
        }
        private static ZonedDateTime GetJobsZonedDateTime(RecurringJobDto rJobDto, DateTimeZone hostDTZ)
        {
            LocalDateTime ldt = LocalDateTime.FromDateTime(rJobDto.NextExecution!.Value);
            ZonedDateTime zdt = ldt.InUtc();
            zdt = zdt.WithZone(hostDTZ);
            return zdt;
        }
    }
}
