using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using HandyHangoutStudios.Parsers;
using HandyHangoutStudios.Parsers.Models;
using HandyHangoutStudios.Parsers.Resolutions;
using Microsoft.Extensions.Logging;
using NodaTime;
using Norm.Database.Entities;
using Norm.Database.Requests;
using Norm.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Norm.Services
{
    public partial class BotService
    {
        private DiscordEmoji? ClockEmoji { get; set; }

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

                IEnumerable<DateTimeV2ModelResult> parserList = Recognizers.RecognizeDateTime(e.Message.Content, DateTimeV2Type.Time, DateTimeV2Type.DateTime);

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

                        DbResult<UserTimeZone> opTimeZoneResult = await this.Mediator.Send(new UserTimeZones.GetUsersTimeZone(msg.Author));
                        if (!opTimeZoneResult.TryGetValue(out UserTimeZone? opTimeZoneEntity))
                        {
                            await reactor.SendMessageAsync("The original poster has not set up a time zone yet.");
                            return;
                        }

                        string opTimeZoneId = opTimeZoneEntity.TimeZoneId;

                        DateTimeZone? opTimeZone = this.TimeZoneProvider.GetZoneOrNull(opTimeZoneId);

                        DbResult<UserTimeZone> reactorTimeZoneResult = await this.Mediator.Send(new UserTimeZones.GetUsersTimeZone(msg.Author));
                        if (!reactorTimeZoneResult.TryGetValue(out UserTimeZone? reactorTimeZoneEntity))
                        {
                            await reactor.SendMessageAsync("You have not set up a time zone yet. Use `time init` to set up your time zone.");
                            return;
                        }

                        string reactorTimeZoneId = reactorTimeZoneEntity.TimeZoneId;

                        DateTimeZone? reactorTimeZone = this.TimeZoneProvider.GetZoneOrNull(reactorTimeZoneId);

                        if (opTimeZone == null || reactorTimeZone == null)
                        {
                            await reactor.SendMessageAsync("There was a problem, please reach out to your bot developer.");
                            return;
                        }

                        ZonedDateTime zonedMessageDateTime = ZonedDateTime.FromDateTimeOffset(msg.CreationTimestamp);
                        DateTime opRefTime = zonedMessageDateTime.WithZone(opTimeZone).ToDateTimeOffset().DateTime;

                        IEnumerable<DateTimeV2ModelResult> parserList = Recognizers.RecognizeDateTime(e.Message.Content, opRefTime, DateTimeV2Type.Time, DateTimeV2Type.DateTime);

                        if (!parserList.Any())
                        {
                            await reactor.SendMessageAsync("This message does not have a recognizable time in it.");
                            return;
                        }

                        DiscordEmbedBuilder reactorTimeEmbed = new DiscordEmbedBuilder().WithTitle("You requested a timezone conversion");

                        

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
