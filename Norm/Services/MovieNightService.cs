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
using Norm.Database.Entities;
using Norm.Database.Requests;
using Norm.Omdb;
using Norm.Omdb.Enums;
using Norm.Omdb.Types;
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

        public MovieNightService(BotService bot, IMediator mediator, IDateTimeZoneProvider timeZoneProvider, OmdbClient omdbClient)
        {
            this.bot = bot;
            this.mediator = mediator;
            this.timeZoneProvider = timeZoneProvider;
            this.omdbClient = omdbClient;
        }

        /// <summary>
        /// Generate the embed with the randomly selected movies and add emojis to allow for voting
        /// </summary>
        /// <param name="movieNightId">ID for the movie night in the data store</param>
        /// <exception cref="ArgumentException">Thrown when an unknown movie night ID is provided</exception>
        public async Task StartVoting(int movieNightId)
        {
            DbResult<GuildMovieNight> movieNightResult = await this.mediator.Send(new GuildMovieNights.GetMovieNight(movieNightId));
            if (!movieNightResult.TryGetValue(out GuildMovieNight? movieNight))
            {
                throw new ArgumentException("Unknown Movie Night ID ");
            }

            DiscordClient client = this.bot.ShardedClient.GetShard(movieNight.GuildId);
            DiscordGuild guild = await client.GetGuildAsync(movieNight.GuildId);
            DiscordChannel channel = guild.GetChannel(movieNight.AnnouncementChannelId);
            DbResult<IEnumerable<GuildMovieSuggestion>> randomSuggestionsResult = await this
                .mediator.Send(new GuildMovieSuggestions.GetRandomGuildMovieSuggestions(guild, movieNight.NumberOfSuggestions, movieNight.MaximumRating));

            if (!randomSuggestionsResult.TryGetValue(out IEnumerable<GuildMovieSuggestion>? randomSuggestions))
                throw new Exception("Something went wrong with getting the random suggestions.");

            string description = AddMovieSuggestionsAndGenerateDescription(client, movieNight, randomSuggestions);

            RecurringJobDto? rJobDto = JobStorage.Current.GetConnection().GetRecurringJobs().FirstOrDefault(x => x.Id == movieNight.MovieNightStartHangfireId); 
            if (rJobDto == null || !rJobDto.NextExecution.HasValue)
            {
                throw new Exception("That Hangfire Job no longer exists or was never scheduled");
            }

            LocalDateTime ldt = LocalDateTime.FromDateTime(rJobDto.NextExecution.Value);
            DbResult<UserTimeZone> hostTimeZoneResult = await this.mediator.Send(new UserTimeZones.GetUsersTimeZone(movieNight.HostId)); 
            if (!hostTimeZoneResult.TryGetValue(out UserTimeZone? hostTimeZone))
            {
                throw new Exception("Host does not have their time zone set up");
            }
            DateTimeZone? hostDTZ = this.timeZoneProvider.GetZoneOrNull(hostTimeZone.TimeZoneId);
            if (hostDTZ == null)
            {
                throw new Exception("Unknown time zone id");
            }

            ZonedDateTime zdt = ldt.InZoneLeniently(this.timeZoneProvider.GetSystemDefault());
            zdt = zdt.WithZone(hostDTZ);

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

        /// <summary>
        /// Determine the number of votes that each movie got and then select the highest ranked movie.
        /// If there is a tie on more than one of the movies, message the movie night creator with an
        /// embed where they will break the tie.
        /// </summary>
        /// <param name="movieNightId">ID for the movie night in the data store</param>
        public async Task CalculateVotes(int movieNightId)
        {
            DbResult<GuildMovieNight> movieNightResult = await this.mediator.Send(new GuildMovieNights.GetMovieNight(movieNightId));
            if (!movieNightResult.TryGetValue(out GuildMovieNight? movieNight))
            {
                throw new ArgumentException("Unknown Movie Night ID ");
            }

            DiscordClient client = this.bot.ShardedClient.GetShard(movieNight.GuildId);
            DiscordGuild guild = await client.GetGuildAsync(movieNight.GuildId);
            DiscordChannel channel = guild.GetChannel(movieNight.AnnouncementChannelId);
            DiscordMessage votingMessage = await channel.GetMessageAsync(movieNight.VotingMessageId ?? throw new Exception("Somehow, some way, the voting message id was null... something done f$*@ed up."));
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

            GuildMovieSuggestion? winningSuggestion;
            DiscordMember host = await guild.GetMemberAsync(movieNight.HostId);
            if (mostReactedReactions.Count > 1)
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
                        TimeSpan.FromMinutes(10)
                    );
                if (tiebreakerReaction.TimedOut)
                {
                    List<GuildMovieSuggestion> tiedList = tiedSuggestions.Select(x => x.First).ToList();
                    winningSuggestion = tiedList[new Random().Next(tiedList.Count)];
                }
                else
                {
                    winningSuggestion = tiedSuggestions.First(x => x.Second.Name == tiebreakerReaction.Result.Emoji.Name).First;
                }
            }
            else
            {
                winningSuggestion = movieNight.MovieNightAndSuggestions.First(mns => mns.EmojiId == mostReactedReactions.First().Value.Emoji.Name).MovieSuggestion;
            }

            RecurringJobDto? rJobDto = JobStorage.Current.GetConnection().GetRecurringJobs().FirstOrDefault(x => x.Id == movieNight.MovieNightStartHangfireId);
            if (rJobDto == null || !rJobDto.NextExecution.HasValue)
            {
                throw new Exception("That Hangfire Job no longer exists or was never scheduled");
            }

            LocalDateTime ldt = LocalDateTime.FromDateTime(rJobDto.NextExecution.Value);
            DbResult<UserTimeZone> hostTimeZoneResult = await this.mediator.Send(new UserTimeZones.GetUsersTimeZone(movieNight.HostId));
            if (!hostTimeZoneResult.TryGetValue(out UserTimeZone? hostTimeZone))
            {
                throw new Exception("Host does not have their time zone set up");
            }
            DateTimeZone? hostDTZ = this.timeZoneProvider.GetZoneOrNull(hostTimeZone.TimeZoneId);
            if (hostDTZ == null)
            {
                throw new Exception("Unknown time zone id");
            }

            ZonedDateTime zdt = ldt.InZoneLeniently(this.timeZoneProvider.GetSystemDefault());
            zdt = zdt.WithZone(hostDTZ);

            OmdbMovie movieInfo = await this.omdbClient.GetByImdbIdAsync(winningSuggestion.ImdbId, omdbPlotOption: OmdbPlotOption.SHORT);

            DiscordEmbedBuilder announceWinnerEmbed = new DiscordEmbedBuilder()
                .WithTitle(movieInfo.Title)
                .WithAuthor(host.DisplayName, iconUrl: host.AvatarUrl)
                .WithDescription($"{movieInfo.Plot}\n[Link to IMDB Page](https://imdb.com/title/{movieInfo.ImdbId}/)")
                .WithThumbnail(movieInfo.Poster)
                .AddField("Rated", movieInfo.Rated?.ToQueryValue() ?? "Unknown", true)
                .AddField("Runtime", movieInfo.Runtime ?? "Unknown", true)
                .AddField("Language", movieInfo.Language ?? "Unknown", true)
                .AddField("Country", movieInfo.Country ?? "Unknown", true)
                .WithFooter("Details provided courtesy of OMDb API");
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
        public void StartMovie(int movieNightId)
        {

        }
    }
}
