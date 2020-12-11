﻿using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Harold.Database;
using Harold.Database.Entities;
using Harold.Services;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using Microsoft.SyndicationFeed;
using Microsoft.SyndicationFeed.Rss;
using System;
using System.Collections.Generic;
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
        public async Task RegisterRoyalRoadFictionAsync(
            CommandContext context, 
            [Description("The channel to announce updates in")]
            DiscordChannel announcementChannel,
            [Description("The role to mention")]
            DiscordRole announcementRole,
            [Description("The RoyalRoad URL")]
            [RemainingText] string royalroadURL)
        {
            // Get fiction id
            Regex fictionIdRegex = new Regex("https://www.royalroad.com/fiction/(?<fictionId>.*)/.*");
            Match fictionIdMatch = fictionIdRegex.Match(royalroadURL);
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
                RoleId = announcementRole?.Id,
                NovelInfoId = fictionInfo.Id
            };

            await psqlContext.GuildNovelRegistrations.AddAsync(register);
            await psqlContext.SaveChangesAsync();
            await context.RespondAsync(
                content: $"I have registered \"{fictionInfo.Name}\" updates to be output in {announcementChannel.Mention} {(announcementRole == null ? "" : $"for members with the {announcementRole.Mention} role")}",
                mentions: Mentions.None);
        }

        [Command("register")]
        public async Task RegisterRoyalRoadFictionForEveryoneAsync(
            CommandContext context,
            [Description("The channel to announce updates in")]
            DiscordChannel announcementChannel,
            [RemainingText] 
            [Description("The RoyalRoad URL")]
            string royalroadURL)
        {
            await this.RegisterRoyalRoadFictionAsync(context, announcementChannel, null, royalroadURL);
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