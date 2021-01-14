using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Harold.Database;
using Harold.Database.Entities;
using Harold.Services;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using Microsoft.SyndicationFeed;
using Microsoft.SyndicationFeed.Rss;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace Harold.Modules
{
    [Group("royalroad")]
    [Aliases("rr")]
    [Description("Commands associated with RoyalRoad web novels")]
    [RequirePermissions(DSharpPlus.Permissions.MentionEveryone)]
    public class RoyalRoadModule : BaseCommandModule
    {
        public BotPsqlContext psqlContext; 
        public RoyalRoadModule(BotPsqlContext psqlContext)
        {
            this.psqlContext = psqlContext;
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
            if (!announcementChannel.PermissionsFor(botMember).HasFlag(DSharpPlus.Permissions.SendMessages))
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
            NovelInfo fictionInfo = await psqlContext.AllNovelInfo.FirstOrDefaultAsync(novel => novel.FictionId == fictionId);
            if (fictionInfo is null)
            {
                fictionInfo = new NovelInfo
                {
                    FictionId = fictionId,
                    SyndicationUri = $"https://www.royalroad.com/fiction/syndication/{fictionId}",
                    FictionUri = $"https://www.royalroad.com/fiction/{fictionId}",
                };
                fictionInfo.MostRecentChapterId = await GetMostRecentChapterId(fictionInfo.SyndicationUri);
                fictionInfo.Name = GetFictionName(fictionInfo.FictionUri).WithHtmlDecoded();
                await psqlContext.AllNovelInfo.AddAsync(fictionInfo);
                await psqlContext.SaveChangesAsync();
                fictionInfo = await psqlContext.AllNovelInfo.FirstOrDefaultAsync(novel => novel.FictionId == fictionId);
            }

            // Register the channel and role 
            GuildNovelRegistration register = new GuildNovelRegistration
            {
                GuildId = context.Guild.Id,
                AnnouncementChannelId = announcementChannel.Id,
                PingEveryone = pingEveryone,
                PingNoOne = pingNoOne,
                RoleId = announcementRole?.Id,
                NovelInfoId = fictionInfo.Id
            };

            await psqlContext.GuildNovelRegistrations.AddAsync(register);
            await psqlContext.SaveChangesAsync();

            string content = new StringBuilder()
                .Append($"I have registered  \"{fictionInfo.Name}\" updates to be output in {announcementChannel.Mention} ")
                .Append(register.PingEveryone ? "for @everyone" : "")
                .Append(register.PingNoOne ? "for everyone but without any ping" : "")
                .Append(register.RoleId != null ? $"for members with the {announcementRole.Mention} role" : "")
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
            GuildNovelRegistration[] allRegisteredFictions = await psqlContext.GuildNovelRegistrations.Where(register => register.GuildId == context.Guild.Id).ToArrayAsync();
            Dictionary<int, NovelInfo> novelInfoDict = await psqlContext.AllNovelInfo.ToDictionaryAsync(info => info.Id);

            StringBuilder pageString = new StringBuilder();
            for (int i = 0; i < allRegisteredFictions.Length; i++)
            {
                pageString.AppendLine($"{i + 1}. {novelInfoDict[allRegisteredFictions[i].NovelInfoId].Name}");
            }

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder().WithTitle("Select the fiction to deregister by typing the number of the fiction");

            InteractivityExtension interactivity = context.Client.GetInteractivity();
            IEnumerable<Page> pages = interactivity.GeneratePagesInEmbed(pageString.ToString(), DSharpPlus.Interactivity.Enums.SplitType.Line, embedbase: builder);

            _ = interactivity.SendPaginatedMessageAsync(context.Channel, context.User, pages);

            InteractivityResult<DiscordMessage> result = await interactivity.WaitForMessageAsync(message => int.TryParse(message.Content, out _));

            if (!result.TimedOut)
            {
                int index = int.Parse(result.Result.Content) - 1;
                NovelInfo delete = novelInfoDict[allRegisteredFictions[index].NovelInfoId];
                this.psqlContext.GuildNovelRegistrations.Remove(allRegisteredFictions[index]);
                await this.psqlContext.SaveChangesAsync();
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
