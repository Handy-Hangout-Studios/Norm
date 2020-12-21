using DSharpPlus;
using DSharpPlus.Entities;
using HandyHangoutStudios.Common.ExtensionMethods;
using Harold.Database;
using Harold.Database.Entities;
using HtmlAgilityPack;
using Microsoft.SyndicationFeed;
using Microsoft.SyndicationFeed.Rss;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace Harold.Services
{
    public class AnnouncementService
    {
        private readonly BotPsqlContext dbContext;
        private readonly IBotService bot;

        public AnnouncementService(BotPsqlContext context, IBotService bot)
        {
            this.dbContext = context;
            this.bot = bot;
        }

        public async Task AnnounceUpdates()
        {
            if (!this.bot.Started)
                return;

            try
            {
                Dictionary<int, ChapterUpdateBucket> buckets = await GetUpdatedChapterBucketDictionary();
                await foreach (GuildNovelRegistration registered in dbContext.GuildNovelRegistrations.AsAsyncEnumerable())
                {
                    if (buckets.ContainsKey(registered.NovelInfoId))
                    {
                        DiscordClient client = this.bot.ShardedClient.GetShard(registered.GuildId);
                        DiscordGuild guild = await client.GetGuildAsync(registered.GuildId);
                        DiscordChannel channel = guild.GetChannel(registered.AnnouncementChannelId);
                        DiscordRole role = registered.RoleId == null ? null : guild.Roles[(ulong)registered.RoleId];
                        string mentionString = $"{role?.Mention ?? "@everyone"}";
                        await channel.SendMessageAsync(content: mentionString, embed: buckets[registered.NovelInfoId].AnnouncementEmbed);
                    }
                }

                await UpdateAllNovelInfos(buckets);
            }
            catch (Exception e)
            {
                await bot.BotDeveloper.SendMessageAsync(embed: new DiscordEmbedBuilder()
                    .WithTitle("Exception occured in Announcement Service")
                    .WithDescription(e.Message)
                    .AddField("Stack Trace", e.StackTrace.Substring(0, e.StackTrace.Length < 500 ? e.StackTrace.Length : 500))
                );
            }
        }

        private async Task UpdateAllNovelInfos(Dictionary<int, ChapterUpdateBucket> buckets)
        {
            if (!buckets.Any()) return;

            foreach ( (int _, ChapterUpdateBucket bucket) in buckets)
            {
                if (bucket.NewTitle is not null)
                {
                    bucket.Novel.Name = bucket.NewTitle;
                }
                bucket.Novel.MostRecentChapterId = bucket.NewMostRecentChapter;
                this.dbContext.Update(bucket.Novel);
            }
            await this.dbContext.SaveChangesAsync();
        }

        private async Task<Dictionary<int, ChapterUpdateBucket>> GetUpdatedChapterBucketDictionary()
        {
            Dictionary<int, ChapterUpdateBucket> allUpdatedChapterBuckets = new Dictionary<int, ChapterUpdateBucket>();

            // Retrieve all updated chapters
            await foreach (NovelInfo novelInfo in this.dbContext.AllNovelInfo.AsAsyncEnumerable())
            {

                ChapterUpdateBucket temp = new ChapterUpdateBucket
                {
                    Novel = novelInfo
                };

                using XmlReader reader = XmlReader.Create(novelInfo.SyndicationUri, new XmlReaderSettings { Async = true, });
                RssFeedReader feedReader = new RssFeedReader(reader);

                while (await feedReader.Read())
                {
                    if (feedReader.ElementType == SyndicationElementType.Item)
                    {
                        ISyndicationItem item = await feedReader.ReadItem();
                        ChapterUpdateItem chapterUpdateItem = item.ToChapterUpdateItem();
                        if (chapterUpdateItem.Id == novelInfo.MostRecentChapterId || chapterUpdateItem.Id == null)
                        {
                            break;
                        }
                            chapterUpdateItem.Description = chapterUpdateItem.Description.FromHtmlToDiscordMarkdown();
                        temp.ChapterUpdateItems.Add(chapterUpdateItem);
                    }
                    else if (feedReader.ElementType == SyndicationElementType.Content)
                    {
                        ISyndicationContent content = await feedReader.ReadContent();
                        if (content.Name.Equals("title") && !content.Value.Equals(temp.Novel.Name))
                        {
                            temp.NewTitle = content.Value;
                        }
                    }
                }
                if (temp.ChapterUpdateItems.Any())
                {
                    allUpdatedChapterBuckets.Add(temp.Novel.Id, temp);
                }
            }

            return allUpdatedChapterBuckets;
        }
    }

    public class ChapterUpdateBucket
    {
        public NovelInfo Novel { get; set; }
        public List<ChapterUpdateItem> ChapterUpdateItems
        {
            get => this.chapterUpdateItems ??= new List<ChapterUpdateItem>();
        }
        public string NewTitle { get; set; }
        public ulong NewMostRecentChapter 
        { 
            get
            {
                if (!this.populated)
                    PopulateFiction();

                return newMostRecentChapter;
            }
        }
        public string FictionCoverUri 
        {
            get
            {
                this.PopulateFiction(); 
                return fictionCoverUri;
            }
        }
        public DiscordEmbed AnnouncementEmbed
        {
            get
            {
                this.PopulateFiction();
                return announcementEmbed ??= this.GetAnnouncementEmbed();
            }
        }

        private bool populated = false;
        private List<ChapterUpdateItem> chapterUpdateItems;
        private DiscordEmbed announcementEmbed;
        private string fictionCoverUri;
        private ulong newMostRecentChapter;

        private DiscordEmbed GetAnnouncementEmbed()
        {
            DiscordEmbedBuilder announcementEmbedBuilder = new DiscordEmbedBuilder()
                .WithTitle($"{(this.NewTitle ?? this.Novel.Name)} just released {(this.ChapterUpdateItems.Count > 1 ? "new chapters!" : "a new chapter!")}");

            if (this.FictionCoverUri != null)
                announcementEmbedBuilder.WithThumbnail(url: this.FictionCoverUri);

            foreach (ChapterUpdateItem item in this.ChapterUpdateItems.OrderBy(item => item.PublishDate))
            {
                announcementEmbedBuilder.AddField(
                    name: $"{item.Title[((this.NewTitle ?? this.Novel.Name).Length + 3)..]}", 
                    value: $"{item.Description.Substring(0, Math.Min(500, item.Description.Length))}{(item.Description.Length > 50 ? "..." : "")}\n[Link to Chapter]({item.Link})"
                );
            }
            
            return announcementEmbedBuilder;
        }

        private void PopulateFiction()
        {
            if (this.populated) return;

            HtmlWeb fictionGet = new HtmlWeb();
            HtmlDocument fictionPage = fictionGet.Load(this.Novel.FictionUri);
            HtmlNodeCollection metaNodes = fictionPage.DocumentNode.SelectNodes("//meta");

            if (metaNodes != null)
            {
                foreach (HtmlNode node in metaNodes)
                {
                    HtmlAttribute nodeContent = node.Attributes["content"];
                    HtmlAttribute nodeProperty = node.Attributes["property"];

                    if (nodeProperty != null && nodeContent != null)
                    {
                        switch (nodeProperty.Value.ToLower())
                        {
                            case "og:image":
                                fictionCoverUri = nodeContent.Value;
                                break;
                        }
                    }
                }
            }

            this.newMostRecentChapter = this.ChapterUpdateItems.OrderByDescending(item => item.PublishDate).FirstOrDefault()?.Id ?? this.Novel.MostRecentChapterId;

            populated = true;
        }
    }

    public class ChapterUpdateItem
    {
        public string Title { get; init; }
        public string Link { get; init; }
        public string Description { get; set; }
        public ulong? Id { get; init; }
        public DateTimeOffset PublishDate { get; init; }
    }

    public static class SyndicationItemExtensionMethods
    {
        public static ChapterUpdateItem ToChapterUpdateItem(this ISyndicationItem item)
        {
            return new ChapterUpdateItem
            {
                Title = item.Title.WithHtmlDecoded(),
                Link = item.Links.First().Uri.AbsoluteUri,
                Description = item.Description.WithHtmlDecoded(),
                Id = item.Id.ToUlong(),
                PublishDate = item.Published,
            };
        }

        private static ulong? ToUlong(this string idString)
        {
            if (ulong.TryParse(idString, out ulong result))
            {
                return result;
            }

            return null;
        }

        public static string WithHtmlDecoded(this string html)
        {
            return System.Net.WebUtility.HtmlDecode(html);
        }
    }
}
