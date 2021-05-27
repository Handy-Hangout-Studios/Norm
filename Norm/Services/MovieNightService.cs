using DSharpPlus;
using DSharpPlus.Entities;
using Hangfire;
using Hangfire.Storage;
using MediatR;
using NodaTime;
using Norm.Database.Entities;
using Norm.Database.Requests;
using Norm.Omdb.Enums;
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

        public MovieNightService(BotService bot, IMediator mediator, IDateTimeZoneProvider timeZoneProvider)
        {
            this.bot = bot;
            this.mediator = mediator;
            this.timeZoneProvider = timeZoneProvider;
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

            await channel.SendMessageAsync(mBuilder);
            await this.mediator.Send(new GuildMovieNights.Update(movieNight));
        }

        private static IEnumerable<DiscordEmoji>? numbers = null;
        private static string AddMovieSuggestionsAndGenerateDescription(DiscordClient client, GuildMovieNight movieNight, IEnumerable<GuildMovieSuggestion> suggestions)
        {
            numbers ??= new List<DiscordEmoji>
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

            if (numbers.Count() < suggestions.Count())
                throw new ArgumentException("Attempted to use more suggestions than is allowed by the system");

            StringBuilder descriptionBuilder = new();
            foreach ((GuildMovieSuggestion gms, DiscordEmoji emoji) in suggestions.Zip(numbers))
            {
                movieNight.MovieNightAndSuggestions.Add(new MovieNightAndSuggestion(movieNight.Id, gms.ImdbId, emoji.Id, gms.GuildId));
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
        public void CalculateVotes(ulong movieNightId)
        {

        }

        /// <summary>
        /// Make an announcement about the movie starting and then begin tracking who shows up over the
        /// next two hours to see if the people who voted showed up to the movie. If they didn't, make 
        /// them not able to vote for the next weeks movie.
        /// </summary>
        /// <param name="movieNightId">ID for the movie night in the data store</param>
        public void StartMovie(ulong movieNightId)
        {

        }
    }
}
