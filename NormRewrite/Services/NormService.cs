using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Norm.DatabaseRewrite.Requests;
using NormRewrite.OptionConfigs;

namespace NormRewrite.Services;

public class NormService : IHostedService
{
    private readonly ILogger<NormService> _logger;
    private readonly DiscordShardedClient _client;
    private readonly IMediator _mediator;
    private readonly CommandsNextConfiguration _commandsConfig;
    private readonly SlashCommandsConfiguration _slashCommandsConfig;
    private readonly InteractivityConfiguration _interactivityConfig;
    private readonly NormConfig _normConfig;
    private readonly IHostEnvironment _environment;

    public NormService(
        ILogger<NormService> logger, 
        ILoggerFactory loggerFactory,
        IServiceProvider serviceProvider,
        IOptions<NormConfig> options,
        IHostEnvironment environment,
        IConfiguration config,
        IMediator mediator
        )
    {
        this._mediator = mediator;
        this._logger = logger;
        this._normConfig = options.Value;
        this._environment = environment;
        this._client = new DiscordShardedClient(new DiscordConfiguration
        {
            Token = this._normConfig.Token,
            TokenType = TokenType.Bot,
            Intents = DiscordIntents.All,
            LoggerFactory = loggerFactory,
            MinimumLogLevel = LogLevel.Trace,
        });
        
        this._commandsConfig = new CommandsNextConfiguration
        {
            Services = serviceProvider,
            EnableDms = true,
            EnableMentionPrefix = true,
            StringPrefixes = new[] { "norm!" },
        };

        this._slashCommandsConfig = new SlashCommandsConfiguration
        {
            Services = serviceProvider,
        };
        
        this._interactivityConfig = new InteractivityConfiguration
        {
            ButtonBehavior = ButtonPaginationBehavior.Disable,
            ResponseBehavior = InteractionResponseBehavior.Respond,
            ResponseMessage = $"Something went wrong when processing that interaction. Please join our support server: {this._normConfig.SupportServerLink}",
        };
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await this._mediator.Send(new Database.Migrate(), cancellationToken);
        IReadOnlyDictionary<int, CommandsNextExtension> commandsExtensionsDictionary = 
            await this._client.UseCommandsNextAsync(this._commandsConfig);
        
        foreach (CommandsNextExtension commands in commandsExtensionsDictionary.Values)
        {
            commands.RegisterCommands(Assembly.GetExecutingAssembly()); 
        }
        
        await this._client.UseInteractivityAsync(this._interactivityConfig);
        
        IReadOnlyDictionary<int, SlashCommandsExtension> slashCommandsExtensionsDictionary = 
            await this._client.UseSlashCommandsAsync(this._slashCommandsConfig);
        
        foreach (SlashCommandsExtension commands in slashCommandsExtensionsDictionary.Values)
        {
            if (this._environment.IsDevelopment())
                commands.RegisterCommands(Assembly.GetExecutingAssembly(), this._normConfig.DevGuildId);
            else 
                commands.RegisterCommands(Assembly.GetExecutingAssembly());
        }
        
        this._client.ClientErrored += this.LogClientErrors;
        await this._client.StartAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await this._client.StopAsync();
    }

    private Task LogClientErrors(DiscordClient client, ClientErrorEventArgs eventArgs)
    {
        this._logger.LogCritical(eventArgs.Exception, "Event Name: {}", eventArgs.EventName);
        return Task.CompletedTask;
    }
}