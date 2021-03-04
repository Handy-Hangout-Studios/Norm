using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using HtmlAgilityPack;
using MediatR;
using Microsoft.SyndicationFeed;
using Microsoft.SyndicationFeed.Rss;
using Norm.Attributes;
using Norm.Database.Entities;
using Norm.Database.Requests;
using Norm.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace Norm.Modules
{
    [Group("royalroad")]
    [Aliases("rr")]
    [Description("Commands associated with RoyalRoad web novels")]
    [BotCategory(BotCategory.WebNovel)]
    public class RoyalRoadModule : BaseCommandModule
    {
        private readonly IMediator mediator;
        public RoyalRoadModule(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [Command("register")]
        [Aliases("r")]
        [Description("Register a channel for update announcements from a RoyalRoad webnovel for specified role, if a role isn't specified this will ping `@everyone`")]
        [RequirePermissions(DSharpPlus.Permissions.MentionEveryone)]
        [RequireGuild]
        public async Task RegisterRoyalRoadFictionWithDiscordRoleAsync(
            CommandContext context,
            [Description("The channel to announce updates in")]
            DiscordChannel announcementChannel,
            [Description("The role to mention")]
            DiscordRole announcementRole,
            [Description("The RoyalRoad URL")]
            [RemainingText] string royalroadUrl)
        {
            await this.RegisterRoyalRoadFictionAsync(context, announcementChannel, announcementRole, false, false, royalroadUrl);
        }

        [Command("register")]
        public async Task RegisterRoyalRoadFictionForEveryoneOrNoOneAsync(
            CommandContext context,
            [Description("The channel to announce updates in")]
            DiscordChannel announcementChannel,
            [Description("Whether to ping everyone or no one. Use everyone to ping everyone or none to ping no one.")]
            string whoToPing,
            [RemainingText]
            [Description("The RoyalRoad URL")]
            string royalroadURL)
        {
            await this.RegisterRoyalRoadFictionAsync(context, announcementChannel, null, whoToPing.ToLower().Equals("everyone"), whoToPing.ToLower().Equals("none"), royalroadURL);
        }

        private async Task RegisterRoyalRoadFictionAsync(CommandContext context, DiscordChannel announcementChannel, DiscordRole announcementRole, bool pingEveryone, bool pingNoOne, string royalroadUrl)
        {
            if (!announcementChannel.PermissionsFor(context.Guild.CurrentMember).HasFlag(DSharpPlus.Permissions.SendMessages | DSharpPlus.Permissions.EmbedLinks))
            {
                await context.RespondAsync($"{context.Member.Mention}, the channel provided doesn't let me send messages. Please try again after you have set permissions such that I can send messages in that channel.");
                return;
            }

            NovelInfo fictionInfo = await this.GetNovelInfoFromUrl(context, royalroadUrl);

            // Register the channel and role 
            DbResult<GuildNovelRegistration> registerResult = await this.mediator.Send(
                new GuildNovelRegistrations.Add(
                    context.Guild.Id,
                    announcementChannel.Id,
                    pingEveryone,
                    pingNoOne,
                    announcementRole?.Id,
                    fictionInfo.Id,
                    null,
                    false
                )
            );

            string content = new StringBuilder()
                .Append($"I have registered  \"{fictionInfo.Name}\" updates to be output in {announcementChannel.Mention} ")
                .Append(registerResult.Value.PingEveryone ? "for @everyone" : "")
                .Append(registerResult.Value.PingNoOne ? "for everyone but without any ping" : "")
                .Append(registerResult.Value.RoleId != null ? $"for members with the {announcementRole.Mention} role" : "")
                .ToString();

            await new DiscordMessageBuilder()
                .WithContent(content)
                .WithAllowedMentions(Mentions.None)
                .SendAsync(context.Channel);
        }

        private async Task<NovelInfo> GetNovelInfoFromUrl(CommandContext context, string url)
        {
            ulong fictionId = await GetFictionId(context, url);

            return await this.GetOrCreateNovelInfo(fictionId);
        }

        private static async Task<ulong> GetFictionId(CommandContext context, string royalroadUrl)
        {
            Regex fictionIdRegex = new("https://www.royalroad.com/fiction/(?<fictionId>.*)/.*");
            Match fictionIdMatch = fictionIdRegex.Match(royalroadUrl);
            if (!fictionIdMatch.Success)
            {
                await context.RespondAsync($"{context.Member.Mention}, the URL provided doesn't match a royalroad fiction URL.");
                throw new Exception();
            }
            return ulong.Parse(fictionIdMatch.Groups["fictionId"].Value);
        }

        private async Task<NovelInfo> GetOrCreateNovelInfo(ulong fictionId)
        {
            // Get or create fiction info
            DbResult<NovelInfo> fictionInfoResult = await this.mediator.Send(new NovelInfos.GetNovelInfo(fictionId));
            NovelInfo fictionInfo = fictionInfoResult.Success ? fictionInfoResult.Value : throw new Exception("Error using NovelInfos.GetNovelInfo(fictionId)");
            if (fictionInfo is null)
            {
                string fictionUri = $"{FictionUri}/{fictionId}";
                string synUri = $"{SyndicationUri}/{fictionId}";
                fictionInfo = (await this.mediator.Send(
                    new NovelInfos.Add(
                        fictionId,
                        GetFictionName(fictionUri).WithHtmlDecoded(),
                        synUri,
                        fictionUri,
                        await GetMostRecentChapterId(synUri)
                     )
                )).Value;
            }

            return fictionInfo;
        }

        [Command("deregister")]
        [Aliases("dr")]
        [Description("Begin the interactive deregistration process to remove a RoyalRoad webnovel announcement from your guild")]
        [RequirePermissions(DSharpPlus.Permissions.MentionEveryone)]
        [RequireGuild]
        public async Task UnregisterRoyalRoadFictionAsync(CommandContext context)
        {
            DbResult<IEnumerable<GuildNovelRegistration>> getNovelRegistrationsResult = await this.mediator.Send(new GuildNovelRegistrations.GetGuildsNovelRegistrations(context.Guild));
            if (!getNovelRegistrationsResult.Success)
            {
                await context.RespondAsync("There was an error getting the Guild's Novel Registrations. An error report has been sent to the developer. DM any extra details that you might find relevant.");
                throw new Exception("error using GetGuildNovelRegistration");
            }
            GuildNovelRegistration[] allRegisteredFictions = getNovelRegistrationsResult.Value.ToArray();

            StringBuilder pageString = new();
            for (int i = 0; i < allRegisteredFictions.Length; i++)
            {
                pageString.AppendLine($"{i + 1}. {allRegisteredFictions[i].NovelInfo.Name}");
            }

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder().WithTitle("Select the fiction to deregister by typing the number of the fiction");

            InteractivityExtension interactivity = context.Client.GetInteractivity();
            IEnumerable<Page> pages = interactivity.GeneratePagesInEmbed(pageString.ToString(), DSharpPlus.Interactivity.Enums.SplitType.Line, embedbase: builder);

            _ = interactivity.SendPaginatedMessageAsync(context.Channel, context.User, pages);

            InteractivityResult<DiscordMessage> result = await interactivity.WaitForMessageAsync(message => int.TryParse(message.Content, out _));

            if (!result.TimedOut)
            {
                int index = int.Parse(result.Result.Content) - 1;
                NovelInfo delete = allRegisteredFictions[index].NovelInfo;
                await this.mediator.Send(new GuildNovelRegistrations.Delete(allRegisteredFictions[index]));
                await context.RespondAsync($"Unregistered {delete.Name}");
            }
        }

        [Group("dm")]
        [Description("The command module for DM based announcements. You must register the novel in a guild in which you are a member. However, you can deregister inside DMs.")]
        public class DM : BaseCommandModule
        {
            private readonly IMediator mediator;
            public DM(IMediator mediator)
            {
                this.mediator = mediator;
            }

            [Command("deregister")]
            [Aliases("dr")]
            [Description("Begin the interactive deregistration process to remove a RoyalRoad webnovel announcement from your DMs")]
            public async Task UnregisterRoyalRoadFictionAsync(CommandContext context)
            {
                DbResult<IEnumerable<GuildNovelRegistration>> getNovelRegistrationsResult = await this.mediator.Send(new GuildNovelRegistrations.GetMemberNovelRegistrations(context.Member));
                if (!getNovelRegistrationsResult.Success)
                {
                    await context.RespondAsync("There was an error getting your Novel Registrations. An error report has been sent to the developer. DM any extra details to the developer that you might find relevant.");
                    throw new Exception("error using GetGuildNovelRegistration");
                }
                GuildNovelRegistration[] allRegisteredFictions = getNovelRegistrationsResult.Value.ToArray();

                StringBuilder pageString = new();
                for (int i = 0; i < allRegisteredFictions.Length; i++)
                {
                    pageString.AppendLine($"{i + 1}. {allRegisteredFictions[i].NovelInfo.Name}");
                }

                DiscordEmbedBuilder builder = new DiscordEmbedBuilder().WithTitle("Select the fiction to deregister by typing the number of the fiction");

                InteractivityExtension interactivity = context.Client.GetInteractivity();
                IEnumerable<Page> pages = interactivity.GeneratePagesInEmbed(pageString.ToString(), DSharpPlus.Interactivity.Enums.SplitType.Line, embedbase: builder);

                _ = interactivity.SendPaginatedMessageAsync(context.Channel, context.User, pages);

                InteractivityResult<DiscordMessage> result = await interactivity.WaitForMessageAsync(message => int.TryParse(message.Content, out _));

                if (!result.TimedOut)
                {
                    int index = int.Parse(result.Result.Content) - 1;
                    NovelInfo delete = allRegisteredFictions[index].NovelInfo;
                    await this.mediator.Send(new GuildNovelRegistrations.Delete(allRegisteredFictions[index]));
                    await context.RespondAsync($"Unregistered {delete.Name}");
                }
            }

            [Command("register")]
            [Aliases("r")]
            [RequireGuild]
            [Description("Register to receive updates about a RoyalRoad novel in your DM's from me")]
            public async Task RegisterRoyalRoadFictionAsync(CommandContext context, [Description("The royalroad url for the novel you'd like to register")][RemainingText] string royalroadUrl)
            {
                NovelInfo fictionInfo = await this.GetNovelInfoFromUrl(context, royalroadUrl);

                try
                {
                    await context.Member.SendMessageAsync($"I have registered  \"{fictionInfo.Name}\" updates to be sent to your DMs!");
                }
                catch (UnauthorizedException)
                {
                    await context.RespondAsync("I'm sorry, but you seem to have your DM's closed on this server, I can't seem to send you a message.");
                    return;
                }
                // Register the channel and role 
                await this.mediator.Send(
                    new GuildNovelRegistrations.Add(
                        context.Guild.Id,
                        context.Channel.Id,
                        false,
                        false,
                        null,
                        fictionInfo.Id,
                        context.Member.Id,
                        true
                    )
                );
            }

            private async Task<NovelInfo> GetNovelInfoFromUrl(CommandContext context, string url)
            {
                ulong fictionId = await GetFictionId(context, url);

                return await this.GetOrCreateNovelInfo(fictionId);
            }

            private static async Task<ulong> GetFictionId(CommandContext context, string royalroadUrl)
            {
                Regex fictionIdRegex = new("https://www.royalroad.com/fiction/(?<fictionId>.*)/.*");
                Match fictionIdMatch = fictionIdRegex.Match(royalroadUrl);
                if (!fictionIdMatch.Success)
                {
                    await context.RespondAsync($"{context.Member.Mention}, the URL provided doesn't match a royalroad fiction URL.");
                    throw new Exception();
                }
                return ulong.Parse(fictionIdMatch.Groups["fictionId"].Value);
            }

            private async Task<NovelInfo> GetOrCreateNovelInfo(ulong fictionId)
            {
                // Get or create fiction info
                DbResult<NovelInfo> fictionInfoResult = await this.mediator.Send(new NovelInfos.GetNovelInfo(fictionId));
                NovelInfo fictionInfo = fictionInfoResult.Success ? fictionInfoResult.Value : throw new Exception("Error using NovelInfos.GetNovelInfo(fictionId)");
                if (fictionInfo is null)
                {
                    string fictionUri = $"{FictionUri}/{fictionId}";
                    string synUri = $"{SyndicationUri}/{fictionId}";
                    fictionInfo = (await this.mediator.Send(
                        new NovelInfos.Add(
                            fictionId,
                            GetFictionName(fictionUri).WithHtmlDecoded(),
                            synUri,
                            fictionUri,
                            await GetMostRecentChapterId(synUri)
                         )
                    )).Value;
                }

                return fictionInfo;
            }
        }

        private static async Task<ulong> GetMostRecentChapterId(string syndicationUri)
        {
            using XmlReader reader = XmlReader.Create(syndicationUri, new XmlReaderSettings { Async = true, });
            RssFeedReader feedReader = new(reader);

            while (await feedReader.Read())
            {
                if (feedReader.ElementType == SyndicationElementType.Item)
                {
                    ISyndicationItem item = await feedReader.ReadItem();
                    return ulong.Parse(item.Id);
                }
            }

            throw new Exception("There was no most recent chapter");
        }

        private static string GetFictionName(string fictionUri)
        {
            HtmlWeb fictionGet = new();
            HtmlDocument fictionPage = fictionGet.Load(fictionUri);

            return fictionPage.DocumentNode
                .Element("html")
                .Element("head")
                .Element("title")
                .InnerHtml;
        }

        private const string RoyalRoadUri = "https://royalroad.com";
        private const string FictionUri = RoyalRoadUri + "/fiction";
        private const string SyndicationUri = FictionUri + "/syndication";
    }
}
