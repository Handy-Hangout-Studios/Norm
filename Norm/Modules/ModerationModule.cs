using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using HandyHangoutStudios.Parsers;
using HandyHangoutStudios.Parsers.Models;
using Hangfire;
using MediatR;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.DateTime;
using NodaTime;
using Norm.Attributes;
using Norm.Database.Entities;
using Norm.Database.Requests;
using Norm.Services;
using Norm.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Norm.Modules
{
    public class ModerationModule : BaseCommandModule
    {
        private readonly IMediator mediator;
        private readonly IClock clock;

        public ModerationModule(IMediator mediator, IClock clock)
        {
            this.mediator = mediator;
            this.clock = clock;
        }

        [Command("warn")]
        [BotCategory("Moderation")]
        [Description("Warn a member and add a record to the guild moderation audit log with the reason for the warning.")]
        [RequireUserPermissions(Permissions.ViewAuditLog)]
        [RequireGuild]
        public async Task WarnMemberAsync(CommandContext context,
            [Description("Guild Member to Warn")]
            DiscordMember member,
            [Description("Reason for warning")]
            [RemainingText]
            string reason)
        {
            DiscordEmbedBuilder successEmbed = new DiscordEmbedBuilder()
                .WithTitle($"You have received a warning!")
                .AddField("Guild:", context.Guild.Name)
                .AddField("Reason:", reason);

            DiscordChannel logChannel = await this.SendModerationEmbedAndGetLogChannel(successEmbed, member, context.Member, context.Guild);

            await this.mediator.Send(new GuildModerationAuditRecords.Add(context.Guild.Id, context.User.Id, member.Id, ModerationActionType.WARN, reason));

            if (logChannel == null)
            {
                return;
            }

            successEmbed = new DiscordEmbedBuilder()
                .WithTitle($"{member.Username} has received a warning!")
                .AddField("Moderator:", context.User.Username)
                .AddField("Reason:", reason)
                .WithFooter($"{this.clock.GetCurrentInstant():g}");

            await logChannel.SendMessageAsync(embed: successEmbed);
            await context.Message.CreateReactionAsync(DiscordEmoji.FromName(context.Client, ":white_check_mark:"));
        }

        [Command("ban")]
        [BotCategory("Moderation")]
        [Description("Permanently ban a member from the guild")]
        [RequirePermissions(Permissions.BanMembers)]
        [RequireGuild]
        public async Task BanMemberAsync(CommandContext context,
            [Description("Guild Member to Ban")]
            DiscordMember member,
            [Description("Number of days worth of their messages to delete")]
            int numDays = 0,
            [Description("Reason for ban")]
            [RemainingText] string reason = null)
        {
            DiscordEmbedBuilder messageEmbed = new DiscordEmbedBuilder()
               .WithTitle($"You have been banned from {context.Guild.Name}!");

            if (reason != null)
            {
                messageEmbed.AddField("Reason:", reason);
            }

            DiscordChannel logChannel = await this.SendModerationEmbedAndGetLogChannel(messageEmbed, member, context.Member, context.Guild);
            await member.BanAsync(numDays, reason);

            await this.mediator.Send(new GuildModerationAuditRecords.Add(context.Guild.Id, context.User.Id, member.Id, ModerationActionType.BAN, reason));

            if (logChannel == null)
            {
                return;
            }

            DiscordEmbedBuilder successEmbed = new DiscordEmbedBuilder()
                .WithTitle($"{member.Username} was banned")
                .AddField("Moderator", context.User.Username)
                .WithFooter($"{this.clock.GetCurrentInstant():g}");

            if (reason != null)
            {
                successEmbed.AddField("Reason:", reason);
            }

            await logChannel.SendMessageAsync(embed: successEmbed);
            await context.Message.CreateReactionAsync(DiscordEmoji.FromName(context.Client, ":white_check_mark:"));
        }

        [Command("ban")]
        public async Task BanMemberNoDeleteAsync(CommandContext context,
            DiscordMember member,
            [RemainingText] string reason = null)
        {
            await this.BanMemberAsync(context, member, 0, reason);
        }

        [Command("tempban")]
        [BotCategory("Moderation")]
        [RequirePermissions(Permissions.BanMembers)]
        [RequireGuild]
        public async Task TempBanMemberAsync(CommandContext context,
            DiscordMember member,
            int numDays = 0,
            [Description("Duration to ban the member for (must be quoted if there are any spaces, however it should work with colloquial language)")]
            string durationOfBan = null,
            [RemainingText]
            string reason = null)
        {
            DateTimeV2ModelResult durationResult = DateTimeRecognizer
                .RecognizeDateTime(durationOfBan, culture: Culture.English)
                .Select(model => model.ToDateTimeV2ModelResult())
                .Where(result => result.TypeName is DateTimeV2Type.Duration)
                .FirstOrDefault();

            if (durationResult == null)
            {
                await context.RespondAsync("There was an error parsing the duration");
                return;
            }

            Duration duration = (Duration)durationResult.Values.FirstOrDefault().Value;
            string durationString = Period.FromSeconds((long)duration.TotalSeconds).AsHumanReadableString();

            DiscordEmbedBuilder messageEmbed = new DiscordEmbedBuilder()
               .WithTitle($"You have been temporarily banned from {context.Guild.Name}!")
               .AddField("Duration", durationString);

            if (reason != null)
            {
                messageEmbed.AddField("Reason:", reason);
            }

            DiscordChannel logChannel = await this.SendModerationEmbedAndGetLogChannel(messageEmbed, member, context.Member, context.Guild);

            await member.BanAsync(numDays, reason);
            await context.Message.CreateReactionAsync(DiscordEmoji.FromName(context.Client, ":white_check_mark:"));

            await this.mediator.Send(new GuildModerationAuditRecords.Add(context.Guild.Id, context.User.Id, member.Id, ModerationActionType.TEMPBAN, reason));

            if (logChannel == null)
            {
                return;
            }

            DiscordEmbedBuilder successEmbed = new DiscordEmbedBuilder()
                .WithTitle($"{member.Username} was temporarily banned")
                .AddField("Moderator", context.User.Username)
                .AddField("Time Banned", durationString)
                .WithFooter($"{this.clock.GetCurrentInstant():g}");

            if (reason != null)
            {
                successEmbed.AddField("Reason:", reason);
            }

            await logChannel.SendMessageAsync(embed: successEmbed);

            string jobId = BackgroundJob.Schedule<ModerationService>((service) =>
                service.UnbanAsync(context.Guild.Id, member.Id),
                duration.ToTimeSpan()
            );

            await this.mediator.Send(new GuildBackgroundJobs.Add(jobId, context.Guild.Id, $"Unban - {member.DisplayName}", this.clock.GetCurrentInstant() + duration, GuildJobType.TEMP_BAN));
        }

        [Command("kick")]
        [BotCategory("Moderation")]
        [Description("Kick a member from the server and send a message explaining why if possible.")]
        [RequirePermissions(Permissions.KickMembers)]
        [RequireGuild]
        public async Task KickMemberAsync(CommandContext context,
            [Description("The member to kick")]
            DiscordMember member,
            [RemainingText]
            [Description("The reason for the kick")]
            string reason = null)
        {
            DiscordEmbedBuilder successEmbed = new DiscordEmbedBuilder()
                .WithTitle($"You have been kicked from {context.Guild.Name}!");

            if (reason != null)
            {
                successEmbed.AddField("Reason:", reason);
            }

            DiscordChannel logChannel = await this.SendModerationEmbedAndGetLogChannel(successEmbed, member, context.Member, context.Guild);

            await member.RemoveAsync(reason);

            await this.mediator.Send(new GuildModerationAuditRecords.Add(context.Guild.Id, context.User.Id, member.Id, ModerationActionType.KICK, reason));
            await context.Message.CreateReactionAsync(DiscordEmoji.FromName(context.Client, ":white_check_mark:"));

            if (logChannel == null)
            {
                return;
            }

            successEmbed = new DiscordEmbedBuilder()
                .WithTitle($"{member.DisplayName} was kicked")
                .AddField("Moderator", context.Member.DisplayName)
                .WithFooter($"{this.clock.GetCurrentInstant():g}");

            if (reason != null)
            {
                successEmbed.AddField("Reason", reason);
            }

            await logChannel.SendMessageAsync(embed: successEmbed);
        }

        [Command("mute")]
        [BotCategory("Moderation")]
        [Description("Mute a member in the server using the `Muted` role and send them a message explaining why if possible. \nCreates the `Muted` role if it doesn't exist.")]
        [RequirePermissions(Permissions.ManageRoles)]
        [RequireBotPermissions(Permissions.ManageChannels)]
        [RequireGuild]
        public async Task MuteMemberAsync(CommandContext context,
            [Description("The member to mute")]
            DiscordMember member,
            [RemainingText]
            [Description("The reason for the mute")]
            string reason = null)
        {
            DiscordRole mutedRole = await GetOrCreateMutedRole(context);

            DiscordEmbedBuilder successEmbed = new DiscordEmbedBuilder()
                .WithTitle($"You have been muted in {context.Guild.Name}!");

            if (reason != null)
            {
                successEmbed.AddField("Reason:", reason);
            }

            DiscordChannel logChannel = await this.SendModerationEmbedAndGetLogChannel(successEmbed, member, context.Member, context.Guild);

            await member.GrantRoleAsync(mutedRole, reason);

            await this.mediator.Send(new GuildModerationAuditRecords.Add(context.Guild.Id, context.User.Id, member.Id, ModerationActionType.MUTE, reason));
            await context.Message.CreateReactionAsync(DiscordEmoji.FromName(context.Client, ":white_check_mark:"));

            if (logChannel == null)
            {
                return;
            }

            successEmbed = new DiscordEmbedBuilder()
                .WithTitle($"{member.DisplayName} was muted")
                .AddField("Moderator", context.Member.DisplayName)
                .WithFooter($"{this.clock.GetCurrentInstant():g}");

            if (reason != null)
            {
                successEmbed.AddField("Reason", reason);
            }

            await logChannel.SendMessageAsync(embed: successEmbed);
        }

        [Command("tempmute")]
        [BotCategory("Moderation")]
        [Description("Temporarily mute a member in the server using the `Muted` role and send them a message explaining why if possible. \n\nCreates the `Muted` role if it doesn't exist.")]
        [RequirePermissions(Permissions.ManageRoles)]
        [RequireBotPermissions(Permissions.ManageChannels)]
        [RequireGuild]
        public async Task TempMuteMemberAsync(CommandContext context,
            [Description("The member to mute")]
            DiscordMember member,
            [Description("Duration to mute the member for (must be quoted if there are any spaces, however it should work with colloquial language)")]
            string durationOfMute,
            [RemainingText]
            [Description("The reason for the mute")]
            string reason = null)
        {
            DateTimeV2ModelResult durationResult = DateTimeRecognizer
                .RecognizeDateTime(durationOfMute, culture: Culture.English)
                .Select(model => model.ToDateTimeV2ModelResult())
                .Where(result => result.TypeName is DateTimeV2Type.Duration)
                .FirstOrDefault();

            if (durationResult == null)
            {
                await context.RespondAsync("There was an error parsing the duration");
                return;
            }

            Duration duration = (Duration)durationResult.Values.FirstOrDefault().Value;
            string durationString = Period.FromSeconds((long)duration.TotalSeconds).AsHumanReadableString();

            DiscordRole mutedRole = await GetOrCreateMutedRole(context);

            DiscordEmbedBuilder successEmbed = new DiscordEmbedBuilder()
                .WithTitle($"You have been temporarily muted in {context.Guild.Name}!")
                .AddField("Duration", durationString);

            if (reason != null)
            {
                successEmbed.AddField("Reason:", reason);
            }

            DiscordChannel logChannel = await this.SendModerationEmbedAndGetLogChannel(successEmbed, member, context.Member, context.Guild);

            await member.GrantRoleAsync(mutedRole, reason);

            await this.mediator.Send(new GuildModerationAuditRecords.Add(context.Guild.Id, context.User.Id, member.Id, ModerationActionType.TEMPMUTE, reason));
            await context.Message.CreateReactionAsync(DiscordEmoji.FromName(context.Client, ":white_check_mark:"));

            string jobId = BackgroundJob.Schedule<ModerationService>(service => service.RemoveRole(context.Guild.Id, member.Id, mutedRole.Id), duration.ToTimeSpan());

            await this.mediator.Send(new GuildBackgroundJobs.Add(jobId, context.Guild.Id, $"Unmute - {member.DisplayName}", this.clock.GetCurrentInstant() + duration, GuildJobType.TEMP_MUTE));

            if (logChannel == null)
            {
                return;
            }

            successEmbed = new DiscordEmbedBuilder()
                .WithTitle($"{member.DisplayName} was muted")
                .AddField("Moderator", context.Member.DisplayName)
                .AddField("Duration", durationString)
                .WithFooter($"{this.clock.GetCurrentInstant():g}");

            if (reason != null)
            {
                successEmbed.AddField("Reason", reason);
            }

            await logChannel.SendMessageAsync(embed: successEmbed);
        }

        [Command("unmute")]
        [BotCategory("Moderation")]
        [Description("Unmute a member in the server and send them a message making them aware of the unmute if possible.")]
        [RequirePermissions(Permissions.ManageRoles)]
        [RequireBotPermissions(Permissions.ManageChannels)]
        [RequireGuild]
        public async Task UnmuteMemberAsync(CommandContext context,
            [Description("The member to unmute")]
            DiscordMember member)
        {
            DiscordRole mutedRole = await GetOrCreateMutedRole(context);

            await member.RevokeRoleAsync(mutedRole);

            await member.SendMessageAsync($"You have just been unmuted in {context.Guild.Name}");
            await context.Message.CreateReactionAsync(DiscordEmoji.FromName(context.Client, ":white_check_mark:"));

            // Add output message to logging channel
            DiscordChannel logChannel = await this.GetGuildLogChannelAsync(context.Guild);

            if (logChannel == null)
            {
                return;
            }

            await logChannel.SendMessageAsync($"{member.DisplayName} was just unmuted.");
        }

        [Command("audit")]
        [BotCategory("Moderation")]
        [Description("View the audit log filtered on the information given")]
        [RequireUserPermissions(Permissions.ViewAuditLog)]
        [RequireGuild]
        public async Task ShowAuditLogVersionOne(CommandContext context,
            [Description("The moderator who took action to filter on.")]
            DiscordUser moderator = null,
            [Description("The member who had action taken against them to filter on.")]
            DiscordUser member = null,
            [Description("The kind of action taken to filter on")]
            ModerationActionType action = ModerationActionType.NONE)
        {

            GuildModerationAuditRecords.GetGuildModerationAuditRecords message = new GuildModerationAuditRecords.GetGuildModerationAuditRecords(context.Guild);

            if (moderator != null)
            {
                message.WithModeratorId(moderator.Id);
            }

            if (member != null)
            {
                message.WithUserId(member.Id);
            }

            if (action != ModerationActionType.NONE)
            {
                message.WithModerationActionType(action);
            }

            List<GuildModerationAuditRecord> auditRecords = (await this.mediator.Send(message)).Value.ToList();
            IReadOnlyCollection<DiscordMember> memberList = await context.Guild.GetAllMembersAsync();
            IDictionary<ulong, DiscordMember> memberDict = memberList.ToDictionary(member => member.Id);

            List<Page> auditPages = GenerateAuditPages(auditRecords, memberDict, context.Client.CurrentUser);

            InteractivityExtension interactivity = context.Client.GetInteractivity();

            await interactivity.SendPaginatedMessageAsync(context.Channel, context.User, auditPages, emojis: null, behaviour: PaginationBehaviour.Ignore, PaginationDeletion.DeleteMessage, timeoutoverride: TimeSpan.FromMinutes(5));
        }

        private static List<Page> GenerateAuditPages(List<GuildModerationAuditRecord> auditRecords, IDictionary<ulong, DiscordMember> memberDict, DiscordUser user)
        {
            List<Page> pages = new List<Page>();
            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
                .WithTitle("Audit Log")
                .WithAuthor(user.Username, iconUrl: user.AvatarUrl);
            foreach (GuildModerationAuditRecord record in auditRecords)
            {
                DiscordMember moderator = memberDict[record.ModeratorUserId];
                DiscordMember instigator = memberDict[record.UserId];
                embedBuilder
                    .AddField("Moderator", moderator?.DisplayName ?? record.ModeratorUserId.ToString())
                    .AddField("Instigator", instigator?.DisplayName ?? record.UserId.ToString())
                    .AddField("Action Taken", record.ModerationAction.ToString());

                if (record.Reason != null)
                {
                    embedBuilder.AddField("Reason", record.Reason);
                }

                pages.Add(new Page()
                {
                    Embed = embedBuilder.Build()
                });

                embedBuilder.ClearFields();
            }

            return pages;
        }

        [Command("audit")]
        public async Task ShowAuditLogVersionTwo(CommandContext context,
            [Description("Specify whether you are filtering on Moderator or Member")]
            string moderatorOrMember,
            DiscordUser member,
            ModerationActionType action = ModerationActionType.NONE)
        {
            switch (moderatorOrMember.ToLower())
            {
                case "moderator":
                case "mod":
                    await this.ShowAuditLogVersionOne(context, member, null, action);
                    break;
                case "member":
                case "mbr":
                case "mem":
                    await this.ShowAuditLogVersionOne(context, null, member, action);
                    break;
                default:
                    throw new ArgumentException($"You attempted to filter on a {moderatorOrMember} which is not an option.");

            };
        }

        [Command("audit")]
        public async Task ShowAuditLogVersionThree(CommandContext context, ModerationActionType action)
        {
            await this.ShowAuditLogVersionOne(context, null, null, action);
        }

        private static async Task<DiscordRole> GetOrCreateMutedRole(CommandContext context)
        {
            DiscordRole mutedRole = context.Guild.Roles.Values.FirstOrDefault(role => role.Name == "Muted");

            if (mutedRole == null)
            {
                await context.RespondAsync("Creating the `Muted` role!");
                mutedRole = await context.Guild.CreateRoleAsync("Muted");
                foreach (DiscordChannel channel in context.Guild.Channels.Values)
                {
                    await channel.AddOverwriteAsync(mutedRole, Permissions.None, Permissions.SendMessages | Permissions.AddReactions, "Create the muted role");
                }
            }

            return mutedRole;
        }

        private async Task<DiscordChannel> SendModerationEmbedAndGetLogChannel(DiscordEmbed embed, DiscordMember member, DiscordMember moderator, DiscordGuild guild)
        {
            DiscordChannel logChannel = await this.GetGuildLogChannelAsync(guild);

            try
            {
                await member.SendMessageAsync(embed: embed);
            }
            catch (UnauthorizedException)
            {
                if (logChannel != null)
                {
                    await logChannel.SendMessageAsync("This user has closed their DMs and so I was not able to message the user.");
                }
                else
                {
                    try
                    {
                        await moderator.SendMessageAsync("This user has closed their DMs and so I was not able to message the user.");
                    }
                    catch (UnauthorizedException)
                    {
                    }
                }
            }

            return logChannel;
        }

        private async Task<DiscordChannel> GetGuildLogChannelAsync(DiscordGuild guild)
        {
            GuildLogChannel guildLogsChannel = (await this.mediator.Send(new GuildLogChannels.GetGuildLogChannel(guild))).Value;
            DiscordChannel logChannel = null;
            if (guildLogsChannel != null)
            {
                logChannel = guild.GetChannel(guildLogsChannel.ChannelId);
            }

            return logChannel;
        }

        [Group("log")]
        [Description("Commands to manage the moderation logging channel")]
        [BotCategory("Moderation")]
        [RequireUserPermissions(Permissions.ViewAuditLog)]
        [RequireGuild]
        public class LogCommands : BaseCommandModule
        {
            private readonly IMediator mediator;

            public LogCommands(IMediator mediator)
            {
                this.mediator = mediator;
            }

            [Command("set")]
            [Description("Set the log channel for this guild\nThis will add the log channel or update the log channel if previously set.")]
            public async Task SetLogChannel(CommandContext context,
                [Description("The channel to set as the log channel for moderation purposes")]
                DiscordChannel channel)
            {
                await this.mediator.Send(new GuildLogChannels.Upsert(context.Guild.Id, channel.Id));

                await channel.SendMessageAsync($"{context.User.Mention}, I have set {channel.Mention} as the logging channel for this guild.");
            }

            [Command("unset")]
            [Description("Unset the log channel. This means that any use of moderation commands will not log for moderator usage.")]
            public async Task UnsetLogChannelAsync(CommandContext context)
            {
                await this.mediator.Send(new GuildLogChannels.Delete(context.Guild));
                await context.RespondAsync($"{context.User.Mention}, I have unset the log channel for this guild.");
            }
        }
    }
}
