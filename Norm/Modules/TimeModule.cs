using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using MediatR;
using NodaTime;
using Norm.Attributes;
using Norm.Database.Entities;
using Norm.Database.Requests;
using Norm.Modules.Exceptions;
using System;
using System.Threading.Tasks;

namespace Norm.Modules
{
    [Group("time")]
    [Description("All commands associated with current time functionality.\n\nWhen used alone, outputs the time of the user mentioned.")]
    [BotCategory(BotCategory.Time)]
    public class TimeModule : BaseCommandModule
    {
        private readonly IMediator mediator;
        private readonly IDateTimeZoneProvider timeZoneProvider;
        private readonly IClock clock;

        public TimeModule(IMediator mediator, IDateTimeZoneProvider timeZoneProvider, IClock clock)
        {
            this.mediator = mediator;
            this.timeZoneProvider = timeZoneProvider;
            this.clock = clock;
        }

        [GroupCommand]
        public async Task CurrentTimeAsync(CommandContext context)
        {
            DbResult<UserTimeZone> memberTimeZoneResult = await this.mediator.Send(new UserTimeZones.GetUsersTimeZone(context.User));
            if (!memberTimeZoneResult.TryGetValue(out UserTimeZone? memberTimeZone))
            {
                throw new TimezoneNotSetupException();
            }

            DateTimeZone memberDateTimeZone = this.timeZoneProvider[memberTimeZone.TimeZoneId];
            this.clock.GetCurrentInstant().InZone(memberDateTimeZone).Deconstruct(out LocalDateTime localDateTime, out _, out _);
            localDateTime.Deconstruct(out _, out LocalTime localTime);
            DiscordEmbed outputEmbed = new DiscordEmbedBuilder()
                .WithAuthor(iconUrl: context.User.AvatarUrl)
                .WithTitle($"{localTime.ToString("t", null)}");

            await context.RespondAsync(embed: outputEmbed);
        }

        [GroupCommand]
        public async Task ExecuteGroupAsync(CommandContext context, [Description("User to request current time for")] DiscordUser member)
        {
            DbResult<UserTimeZone> memberTimeZoneResult = (await this.mediator.Send(new UserTimeZones.GetUsersTimeZone(member)));
            if (!memberTimeZoneResult.TryGetValue(out UserTimeZone? memberTimeZone))
            {
                await context.RespondAsync("This user doesn't have a timezone set up. Please try again after the mentioned user has set up their timezone using `time init`");
                return;
            }

            DateTimeZone memberDateTimeZone = this.timeZoneProvider[memberTimeZone.TimeZoneId];
            this.clock.GetCurrentInstant().InZone(memberDateTimeZone).Deconstruct(out LocalDateTime localDateTime, out _, out _);
            localDateTime.Deconstruct(out _, out LocalTime localTime);
            DiscordEmbed outputEmbed = new DiscordEmbedBuilder()
                .WithAuthor(iconUrl: member.AvatarUrl)
                .WithTitle($"{localTime.ToString("t", null)}");

            await context.RespondAsync(embed: outputEmbed);
        }

        [Command("init")]
        [Description("Perform initial set-up of user's timezone.")]
        public async Task InitializeTimeZoneAsync(CommandContext context)
        {
            if ((await this.mediator.Send(new UserTimeZones.GetUsersTimeZone(context.User))).TryGetValue(out UserTimeZone? _))
            {
                await context.RespondAsync(
                    $"{context.User.Mention}, you already have a timezone set up. To update your timezone please type `time update`.");
                return;
            }

            await context.RespondAsync(
                "Please navigate to https://kevinnovak.github.io/Time-Zone-Picker/ and select your timezone. After you do please hit the copy button and paste the contents into the chat.");
            InteractivityExtension interactivity = context.Client.GetInteractivity();
            InteractivityResult<DiscordMessage> result =
                await interactivity.WaitForMessageAsync(msg => msg.Author.Equals(context.Message.Author),
                    TimeSpan.FromMinutes(1));

            if (!result.TimedOut)
            {
                DateTimeZone? test = this.timeZoneProvider.GetZoneOrNull(result.Result.Content);
                if (test != null)
                {
                    await this.mediator.Send(new UserTimeZones.Add(context.User, result.Result.Content));
                    await context.RespondAsync($"I set your timezone as {result.Result.Content} in all guilds I am a member of.");
                }
                else
                {
                    await context.RespondAsync("You provided me with an invalid timezone. Try again by typing `time init`.");
                }
            }
            else
            {
                await context.RespondAsync(
                    "You waited too long to respond. Try again by typing `time init`.");
            }
        }

        [Command("update")]
        [Description("Perform the time zone update process for the user who called update.")]
        public async Task UpdateTimeZone(CommandContext context)
        {
            DbResult<UserTimeZone> memberTimeZoneResult = await this.mediator.Send(new UserTimeZones.GetUsersTimeZone(context.User));
            if (!memberTimeZoneResult.TryGetValue(out UserTimeZone? memberTimeZone))
            {
                throw new TimezoneNotSetupException();
            }
            await context.RespondAsync(
                "Please navigate to https://kevinnovak.github.io/Time-Zone-Picker/ and select your timezone. After you do please hit the copy button and paste the contents into the chat.");
            InteractivityExtension interactivity = context.Client.GetInteractivity();
            InteractivityResult<DiscordMessage> result = await interactivity.WaitForMessageAsync(msg => msg.Author.Equals(context.Message.Author));

            if (!result.TimedOut)
            {
                DateTimeZone? test = this.timeZoneProvider.GetZoneOrNull(result.Result.Content);
                if (test != null)
                {
                    memberTimeZone.TimeZoneId = result.Result.Content;
                    await this.mediator.Send(new UserTimeZones.Update(memberTimeZone));
                    await context.RespondAsync(
                        $"I updated your timezone to {result.Result.Content} in all guilds I am a member of.");
                }
                else
                {
                    await context.RespondAsync("You provided me with an invalid timezone. Try again by typing `time update`.");
                }
            }
            else
            {
                await context.RespondAsync(
                    "You waited too long to respond. Try again by typing `time update`.");
            }
        }

        [Command("unregister")]
        public async Task UnregisterTimeZone(CommandContext context)
        {
            DbResult<UserTimeZone> timeZoneResult = await this.mediator.Send(new UserTimeZones.GetUsersTimeZone(context.User));
            if (!timeZoneResult.TryGetValue(out UserTimeZone? timeZone))
            {
                await context.RespondAsync("You don't have a timezone registered to unregister.");
                return;
            }
            await this.mediator.Send(new UserTimeZones.Delete(timeZone));
            await context.RespondAsync("Ok, I've unregistered your timezone from my databases. This means that it has been completely deleted.");
        }
    }
}
