using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using Hangfire;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NodaTime;
using Norm.Configuration;
using Norm.Database.Entities;
using Norm.Database.Requests;
using Norm.Formatters;
using Norm.Modules;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Norm.Services
{
    public partial class BotService
    {
        // Client and Extensions
        public DiscordShardedClient ShardedClient { get; private set; }
        private IReadOnlyDictionary<int, CommandsNextExtension>? commandsDict;
        private IReadOnlyDictionary<int, InteractivityExtension>? interactivityDict;

        // Shared between events
        public DiscordMember? BotDeveloper { get; private set; }
        private ILogger Logger { get; }
        private IMediator Mediator { get; }
        private IDateTimeZoneProvider TimeZoneProvider { get; }

        // Configurations
        private readonly BotOptions config;
        private readonly DiscordConfiguration clientConfig;
        private readonly CommandsNextConfiguration commandsConfig;
        private readonly InteractivityConfiguration interactivityConfig;
        public IMemoryCache PrefixCache { get; }

        // Public properties
        [MemberNotNullWhen(true, nameof(BotDeveloper))]
        public bool Started { get; private set; }

        public BotService(
            IOptions<BotOptions> options,
            ILoggerFactory factory,
            IServiceProvider provider,
            IMediator mediator,
            IDateTimeZoneProvider timeZoneProvider)
        {
            this.Started = false;
            this.config = options.Value;
            this.Logger = factory.CreateLogger<BotService>();
            this.Mediator = mediator;
            this.TimeZoneProvider = timeZoneProvider;
            MemoryCacheOptions memCacheOpts = new()
            {
                SizeLimit = 1000,
                CompactionPercentage = .25,
            };
            this.PrefixCache = new MemoryCache(memCacheOpts, factory);

            #region Client Config
            this.clientConfig = new DiscordConfiguration
            {
                Token = this.config.BotToken,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.All,
                LoggerFactory = factory,
                MinimumLogLevel = LogLevel.Trace,
            };
            #endregion

            #region Commands Module Config
            this.commandsConfig = new CommandsNextConfiguration
            {
                Services = provider,
                EnableDms = this.config.EnableDms,
                EnableMentionPrefix = this.config.EnableMentionPrefix
            };

            if (this.config.EnablePrefixResolver)
            {
                this.commandsConfig.PrefixResolver = this.CheckForPrefix;
            }
            else
            {
                this.commandsConfig.StringPrefixes = this.config.Prefixes;
            }
            #endregion

            #region Interactivity Module Config
            this.interactivityConfig = new InteractivityConfiguration
            {
                PaginationBehaviour = PaginationBehaviour.Ignore,
                PaginationDeletion = PaginationDeletion.KeepEmojis,
                PollBehaviour = PollBehaviour.DeleteEmojis,
                Timeout = TimeSpan.FromMinutes(5),
            };
            #endregion

            this.ShardedClient = new DiscordShardedClient(this.clientConfig);
        }

        public async Task StartAsync()
        {
            this.commandsDict = await this.ShardedClient.UseCommandsNextAsync(this.commandsConfig);

            this.interactivityDict = await this.ShardedClient.UseInteractivityAsync(this.interactivityConfig);

            foreach (CommandsNextExtension commands in this.commandsDict.Values)
            {
                commands.RegisterCommands<GeneralModule>();
                commands.RegisterCommands<RoyalRoadModule>();
                commands.RegisterCommands<TimeModule>();
                commands.RegisterCommands<EventModule>();
                commands.RegisterCommands<PrefixModule>();
                commands.RegisterCommands<ModerationModule>();
                commands.RegisterCommands<WelcomeMessageSettingsModule>();
                commands.RegisterCommands<FunModule>();
                commands.RegisterCommands<EvaluationModule>();
                commands.RegisterCommands<TestModule>();

                commands.CommandErrored += ChecksFailedError;
                commands.CommandErrored += this.CheckCommandExistsError;
                commands.CommandErrored += this.LogExceptions;

                commands.SetHelpFormatter<CategoryHelpFormatter>();

                commands.RegisterConverter(new EnumConverter<ModerationActionType>());
            }

            this.ShardedClient.Ready += this.ShardedClient_UpdateStatus;
            this.ShardedClient.GuildDownloadCompleted += this.ShardedClient_GuildDownloadCompleted;

            //this.ShardedClient.MessageCreated += this.CheckForDate;
            this.ShardedClient.MessageReactionAdded += this.SendAdjustedDate;

            this.ShardedClient.GuildMemberAdded += this.SendWelcomeMessage;

            await this.ShardedClient.StartAsync();
        }

        public async Task StopAsync()
        {
            this.Started = false;
            await this.ShardedClient.StopAsync();
        }

        private async Task ShardedClient_UpdateStatus(DiscordClient sender, ReadyEventArgs e)
        {
            this.Started = true;
            await this.ShardedClient.UpdateStatusAsync(new DiscordActivity("^help", ActivityType.Watching));
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        private async Task ShardedClient_GuildDownloadCompleted(DiscordClient client, GuildDownloadCompletedEventArgs args)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            _ = Task.Run(async () =>
            {
                DiscordGuild botDevGuild = await client.GetGuildAsync(this.config.DevGuildId);
                this.BotDeveloper = await botDevGuild.GetMemberAsync(this.config.DevId);
                this.ClockEmoji = DiscordEmoji.FromName(client, ":clock:");
                RecurringJob.AddOrUpdate<AnnouncementService>(service => service.AnnounceUpdates(), "0/15 * * * *");

                await this.BotDeveloper.SendMessageAsync("I'm up and running Prof. :smile:");
            });
        }

        private async Task<int> CheckForPrefix(DiscordMessage msg)
        {
            bool isDm = msg.Channel.Guild is null;
            int prefixPos;
            if (!isDm)
            {
                prefixPos = await this.CheckGuildPrefixes(msg);
                return prefixPos;
            }

            prefixPos = msg.GetStringPrefixLength("^");
            if (prefixPos != -1)
            {
                return prefixPos;
            }

            return isDm ? 0 : -1;
        }

        private async Task<int> CheckGuildPrefixes(DiscordMessage msg)
        {
            if (!this.PrefixCache.TryGetValue(msg.Channel.Guild.Id, out GuildPrefix[] guildPrefixes))
            {
                DbResult<IEnumerable<GuildPrefix>> guildPrefixesResult = await this.Mediator.Send(new GuildPrefixes.GetGuildsPrefixes(msg.Channel.Guild));
                if (!guildPrefixesResult.TryGetValue(out IEnumerable<GuildPrefix>? guildPrefixesEnumerable))
                {
                    guildPrefixes = Array.Empty<GuildPrefix>();
                }
                else
                {
                    guildPrefixes = guildPrefixesEnumerable.ToArray();
                }

                MemoryCacheEntryOptions entryOpts = new()
                {
                    SlidingExpiration = TimeSpan.FromMinutes(5),
                    AbsoluteExpiration = DateTime.Now + TimeSpan.FromDays(1),
                    Size = guildPrefixes.Length,
                };

                this.PrefixCache.Set(msg.Channel.Guild.Id, guildPrefixes, entryOpts);
            }

            foreach (GuildPrefix prefix in guildPrefixes)
            {
                int prefixPos = msg.GetStringPrefixLength(prefix.Prefix);
                if (prefixPos != -1)
                {
                    return prefixPos;
                }
            }

            return guildPrefixes.Any() ? -1 : msg.GetStringPrefixLength("^");
        }
    }
}
