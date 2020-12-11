using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Hangfire;
using Harold.Configuration;
using Harold.Modules;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;

namespace Harold.Services
{
    public class BotService : IBotService
    {
        // Client and Extensions
        public DiscordShardedClient ShardedClient { get; private set; }
        private IReadOnlyDictionary<int, CommandsNextExtension> commandsDict;

        // Shared between events
        private DiscordMember botDeveloper;

        // Configurations
        private readonly BotConfig config;
        private readonly DiscordConfiguration clientConfig;
        private readonly CommandsNextConfiguration commandsConfig;

        // Public properties
        public bool Started { get; private set; }

        public BotService(IOptions<BotConfig> options, ILoggerFactory factory, IServiceProvider provider)
        {
            this.Started = false;
            this.config = options.Value;

            #region Client Config
            this.clientConfig = new DiscordConfiguration
            {
                Token = this.config.BotToken,
                TokenType = TokenType.Bot,
                MinimumLogLevel = LogLevel.Information,
                LoggerFactory = factory,
                Intents = DiscordIntents.AllUnprivileged,
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
                this.commandsConfig.PrefixResolver = PrefixResolver;
            else
                this.commandsConfig.StringPrefixes = this.config.Prefixes;
            #endregion
        }

        public async Task StartAsync()
        {
            this.ShardedClient = new DiscordShardedClient(clientConfig);

            this.commandsDict = await this.ShardedClient.UseCommandsNextAsync(this.commandsConfig);

            foreach (CommandsNextExtension commands in this.commandsDict.Values)
            {
                commands.RegisterCommands<GeneralModule>();
                commands.RegisterCommands<RoyalRoadModule>();
            }

            this.ShardedClient.Ready += ShardedClient_UpdateStatus;
            this.ShardedClient.GuildDownloadCompleted += ShardedClient_GuildDownloadCompleted;

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
            await this.ShardedClient.UpdateStatusAsync(new DiscordActivity("RoyalRoad for updates", ActivityType.Watching));
        }

        private async Task ShardedClient_GuildDownloadCompleted(DiscordClient client, GuildDownloadCompletedEventArgs args)
        {
            _ = Task.Run(async () =>
            {
                DiscordGuild botDevGuild = await client.GetGuildAsync(this.config.DevGuildId);
                this.botDeveloper = await botDevGuild.GetMemberAsync(this.config.DevId);
                RecurringJob.AddOrUpdate<AnnouncementService>(service => service.AnnounceUpdates(), "*/15 * * * *");
                await this.botDeveloper.SendMessageAsync("Announcements have been started");
            });
        }

        private Task<int> PrefixResolver(DiscordMessage msg)
        {
            throw new NotImplementedException();
        }
    }
}
