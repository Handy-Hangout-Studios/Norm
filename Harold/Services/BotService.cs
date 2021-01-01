using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using Hangfire;
using Harold.Configuration;
using Harold.Database;
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
    public partial class BotService : IBotService
    {
        // Client and Extensions
        public DiscordShardedClient ShardedClient { get; private set; }
        private IReadOnlyDictionary<int, CommandsNextExtension> commandsDict;
        private IReadOnlyDictionary<int, InteractivityExtension> interactivityDict;

        // Shared between events
        public DiscordMember BotDeveloper { get; set; }

        // Configurations
        private readonly BotConfig config;
        private readonly BotPsqlContext botPsqlContext;
        private readonly DiscordConfiguration clientConfig;
        private readonly CommandsNextConfiguration commandsConfig;
        private readonly InteractivityConfiguration interactivityConfig;

        // Public properties
        public bool Started { get; private set; }

        public BotService(IOptions<BotConfig> options, ILoggerFactory factory, BotPsqlContext botPsqlContext, IServiceProvider provider)
        {
            this.Started = false;
            this.config = options.Value;
            this.botPsqlContext = botPsqlContext;

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

            #region Interactivity Module Config
            this.interactivityConfig = new InteractivityConfiguration
            {
                PaginationBehaviour = PaginationBehaviour.Ignore,
                PaginationDeletion = PaginationDeletion.KeepEmojis,
                PollBehaviour = PollBehaviour.DeleteEmojis,
                Timeout = TimeSpan.FromMinutes(5),
            };
            #endregion
        }

        public async Task StartAsync()
        {
            this.ShardedClient = new DiscordShardedClient(clientConfig);

            this.commandsDict = await this.ShardedClient.UseCommandsNextAsync(this.commandsConfig);

            this.interactivityDict = await this.ShardedClient.UseInteractivityAsync(this.interactivityConfig);

            foreach (CommandsNextExtension commands in this.commandsDict.Values)
            {
                commands.RegisterCommands<GeneralModule>();
                commands.RegisterCommands<RoyalRoadModule>();
                commands.CommandErrored += this.Commands_CommandErrored;
            }

            this.ShardedClient.Ready += ShardedClient_UpdateStatus;
            this.ShardedClient.GuildDownloadCompleted += ShardedClient_GuildDownloadCompleted;
            //this.ShardedClient.MessageReactionAdded += ShardedClient_ReactionRoleAdd;
            //this.ShardedClient.MessageReactionRemoved += ShardedClient_ReactionRoleRemove;

            await this.ShardedClient.StartAsync();
        }

        private async Task Commands_CommandErrored(CommandsNextExtension sender, CommandErrorEventArgs e)
        {
            try
            {
                await BotDeveloper.SendMessageAsync(embed: new DiscordEmbedBuilder()
                        .WithTitle("Exception occured in Announcement Service")
                        .WithDescription(e.Exception.Message)
                        .AddField("Stack Trace", e.Exception.StackTrace.Substring(0, e.Exception.StackTrace.Length < 500 ? e.Exception.StackTrace.Length : 500))
                    );
            }
            catch
            {
                this.ShardedClient.Logger.Log(LogLevel.Error, "Error in sending exception message to bot developer");
            }
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

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously which isn't a problem in this case
        private async Task ShardedClient_GuildDownloadCompleted(DiscordClient client, GuildDownloadCompletedEventArgs args)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            _ = Task.Run(async () =>
            {
                DiscordGuild botDevGuild = await client.GetGuildAsync(this.config.DevGuildId);
                this.BotDeveloper = await botDevGuild.GetMemberAsync(this.config.DevId);
                RecurringJob.AddOrUpdate<AnnouncementService>(service => service.AnnounceUpdates(), "0/15 * * * *");

                await this.BotDeveloper.SendMessageAsync("Announcements have been started");
            });
        }

        private Task<int> PrefixResolver(DiscordMessage msg)
        {
            throw new NotImplementedException();
        }
    }
}
