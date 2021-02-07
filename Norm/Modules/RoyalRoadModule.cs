using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Norm.Database.Entities;
using Norm.Services;
using HtmlAgilityPack;
using Microsoft.SyndicationFeed;
using Microsoft.SyndicationFeed.Rss;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using MediatR;
using Norm.Database.Requests;
using Norm.Attributes;

namespace Norm.Modules
{
    [Group("royalroad")]
    [Aliases("rr")]
    [Description("Commands associated with RoyalRoad web novels")]
    [RequirePermissions(DSharpPlus.Permissions.MentionEveryone)]
    [BotCategory("Events and Announcements")]
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
            DiscordMember botMember = await context.Guild.GetMemberAsync(context.Client.CurrentUser.Id);
            if (!announcementChannel.PermissionsFor(botMember).HasFlag(DSharpPlus.Permissions.SendMessages | DSharpPlus.Permissions.EmbedLinks))
            {
                await context.RespondAsync($"{context.Member.Mention}, the channel provided doesn't let me send messages. Please try again after you have set permissions such that I can send messages in that channel.");
                return;
            }

            // Get fiction id
            Regex fictionIdRegex = new Regex("https://www.royalroad.com/fiction/(?<fictionId>.*)/.*");
            Match fictionIdMatch = fictionIdRegex.Match(royalroadUrl);
            if (!fictionIdMatch.Success)
            {
                await context.RespondAsync($"{context.Member.Mention}, the URL provided doesn't match a royalroad fiction URL.");
                return;
            }
            ulong fictionId = ulong.Parse(fictionIdMatch.Groups["fictionId"].Value);

            // Get or create fiction info
            var fictionInfoResult = await this.mediator.Send(new NovelInfos.GetNovelInfo(fictionId));
            NovelInfo fictionInfo = fictionInfoResult.Success ? fictionInfoResult.Value : throw new Exception("Error using NovelInfos.GetNovelInfo(fictionId)");
            if (fictionInfo is null)
            {
                string fictionUri = $"https://www.royalroad.com/fiction/{fictionId}";
                string synUri = $"https://www.royalroad.com/fiction/{fictionId}";
                fictionInfo.MostRecentChapterId = await GetMostRecentChapterId(synUri);
                fictionInfo = (await this.mediator.Send(
                    new NovelInfos.Add(
                        fictionId,
                        GetFictionName(fictionInfo.FictionUri).WithHtmlDecoded(),
                        fictionUri,
                        synUri,
                        await GetMostRecentChapterId(synUri)
                     )
                )).Value;
            }

            // Register the channel and role 
            var registerResult = await this.mediator.Send(
                new GuildNovelRegistrations.Add(
                    context.Guild.Id,
                    announcementChannel.Id,
                    pingEveryone,
                    pingNoOne,
                    announcementRole?.Id,
                    fictionInfo.Id
                )
            );



            string content = new StringBuilder()
                .Append($"I have registered  \"{fictionInfo.Name}\" updates to be output in {announcementChannel.Mention} ")
                .Append(registerResult.Value.PingEveryone ? "for @everyone" : "")
                .Append(registerResult.Value.PingNoOne ? "for everyone but without any ping" : "")
                .Append(registerResult.Value.RoleId != null ? $"for members with the {announcementRole.Mention} role" : "")
                .ToString();

            await context.RespondAsync(
                content: content,
                mentions: Mentions.None);
        }

        [Command("deregister")]
        [Aliases("dr")]
        [Description("Begin the interactive deregistration process to remove a RoyalRoad webnovel announcement from your guild")]
        public async Task UnregisterRoyalRoadFictionAsync(CommandContext context)
        {
            var getNovelRegistrationsResult = await this.mediator.Send(new GuildNovelRegistrations.GetGuildsNovelRegistrations(context.Guild));
            if (!getNovelRegistrationsResult.Success)
            {
                await context.RespondAsync("There was an error getting the Guild's Novel Registrations. An error report has been sent to the developer. DM any extra details that you might find relevant.");
                throw new Exception("error using GetGuildNovelRegistration");
            }
            GuildNovelRegistration[] allRegisteredFictions = getNovelRegistrationsResult.Value.ToArray();

            StringBuilder pageString = new StringBuilder();
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

        private static async Task<ulong> GetMostRecentChapterId(string syndicationUri)
        {
            using XmlReader reader = XmlReader.Create(syndicationUri, new XmlReaderSettings { Async = true, });
            RssFeedReader feedReader = new RssFeedReader(reader);

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
            HtmlWeb fictionGet = new HtmlWeb();
            HtmlDocument fictionPage = fictionGet.Load(fictionUri);

            return fictionPage.DocumentNode
                .Element("html")
                .Element("head")
                .Element("title")
                .InnerHtml;
        }
    }
}
