using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using HandyHangoutStudios.Parsers;
using HandyHangoutStudios.Parsers.Models;
using HandyHangoutStudios.Parsers.Resolutions;
using Microsoft.Extensions.Logging;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.DateTime;
using NodaTime;
using Norm.Database.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Norm.Services
{
    public partial class BotService
    {
        private DiscordEmoji ClockEmoji { get; set; }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        private async Task CheckForDate(DiscordClient c, MessageCreateEventArgs e)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            if (e.Author.IsBot)
            {
                return;
            }

            _ = Task.Run(async () =>
            {
                IEnumerable<DateTimeV2ModelResult> parserList = DateTimeRecognizer.RecognizeDateTime(e.Message.Content, culture: Culture.English)
                .Select(x => x.ToDateTimeV2ModelResult()).Where(x => x.TypeName is DateTimeV2Type.Time or DateTimeV2Type.DateTime);

                if (parserList.Any())
                {
                    await e.Message.CreateReactionAsync(this.ClockEmoji);
                }
            });
        }

        private async Task SendAdjustedDate(DiscordClient c, MessageReactionAddEventArgs e)
        {
            if (e.User.IsBot)
            {
                return;
            }

            DiscordChannel channel = await c.GetChannelAsync(e.Channel.Id);
            _ = Task.Run(async () =>
            {
                if (e.Emoji.Equals(this.ClockEmoji))
                {
                    try
                    {
                        DiscordMember reactor = (DiscordMember)e.User;
                        DiscordMessage msg = await channel.GetMessageAsync(e.Message.Id);


                        ;
                        string opTimeZoneId = (await this.Mediator.Send(new UserTimeZones.GetUsersTimeZone(msg.Author))).Value?.TimeZoneId;

                        if (opTimeZoneId is null)
                        {
                            await reactor.SendMessageAsync("The original poster has not set up a time zone yet.");
                            return;
                        }

                        DateTimeZone opTimeZone = this.TimeZoneProvider.GetZoneOrNull(opTimeZoneId);
                        ZonedDateTime zonedMessageDateTime = ZonedDateTime.FromDateTimeOffset(msg.CreationTimestamp);
                        DateTime opRefTime = zonedMessageDateTime.WithZone(opTimeZone).ToDateTimeOffset().DateTime;

                        IEnumerable<DateTimeV2ModelResult> parserList = DateTimeRecognizer.RecognizeDateTime(msg.Content, culture: Culture.English, refTime: opRefTime)
                            .Select(x => x.ToDateTimeV2ModelResult()).Where(x => x.TypeName is DateTimeV2Type.Time or DateTimeV2Type.DateTime);

                        if (!parserList.Any())
                        {
                            await reactor.SendMessageAsync("Hey, you're stupid, stop trying to react to messages that don't have times in them.");
                            return;
                        }

                        DiscordEmbedBuilder reactorTimeEmbed = new DiscordEmbedBuilder().WithTitle("You requested a timezone conversion");

                        string reactorTimeZoneId = (await this.Mediator.Send(new UserTimeZones.GetUsersTimeZone(e.User))).Value?.TimeZoneId;

                        if (reactorTimeZoneId is null)
                        {
                            await reactor.SendMessageAsync("You have not set up a time zone yet. Use `time init` to set up your time zone.");
                            return;
                        }
                        DateTimeZone reactorTimeZone = this.TimeZoneProvider.GetZoneOrNull(reactorTimeZoneId);
                        if (opTimeZone == null || reactorTimeZone == null)
                        {
                            await reactor.SendMessageAsync("There was a problem, please reach out to your bot developer.");
                            return;
                        }

                        IEnumerable<(string, DateTimeV2Value)> results = parserList.SelectMany(x => x.Values.Select(y => (x.Text, y)));
                        foreach ((string parsedText, DateTimeV2Value result) in results)
                        {
                            string outputString;
                            if (result.Type is DateTimeV2Type.Time)
                            {
                                LocalTime localParsedTime = (LocalTime)result.Value;
                                LocalDateTime localParsedDateTime = localParsedTime.On(zonedMessageDateTime.LocalDateTime.Date);
                                ZonedDateTime zonedOpDateTime = localParsedDateTime.InZoneStrictly(opTimeZone);
                                ZonedDateTime zonedReactorDateTime = zonedOpDateTime.WithZone(reactorTimeZone);
                                outputString = zonedReactorDateTime.LocalDateTime.TimeOfDay.ToString("t", null);
                            }
                            else
                            {
                                LocalDateTime localParsedDateTime = (LocalDateTime)result.Value;
                                ZonedDateTime zonedOpDateTime = localParsedDateTime.InZoneStrictly(opTimeZone);
                                ZonedDateTime zonedReactorDateTime = zonedOpDateTime.WithZone(reactorTimeZone);
                                outputString = zonedReactorDateTime.LocalDateTime.ToString("g", null);
                            }

                            reactorTimeEmbed
                                .AddField("Poster's Time", $"\"{parsedText}\"")
                                .AddField("Your time", $"{outputString}");
                        }
                        await reactor.SendMessageAsync(embed: reactorTimeEmbed);
                    }
                    catch (Exception exception)
                    {
                        this.Logger.Log(LogLevel.Error, exception, "Error in sending reactor the DM");
                    }
                }
            });
        }
    }
}
