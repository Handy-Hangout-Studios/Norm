using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NormRewrite.ExtensionMethods;
using NormRewrite.OptionConfigs;

namespace NormRewrite.SlashCommands;

public class GeneralModule : ApplicationCommandModule
{
    private readonly NormConfig _normConfig;
    private readonly bool _isDevelopment;
    
    public GeneralModule(IOptions<NormConfig> options, IHostEnvironment environment)
    {
        this._normConfig = options.Value;
        this._isDevelopment = environment.IsDevelopment();
    }
    
    [SlashCommand("invite", "Invite the bot to your server!")]
    public async Task InviteCommand(InteractionContext context)
    {
        const Permissions minPerms = Permissions.SendMessages | Permissions.AccessChannels | 
                                     Permissions.ChangeNickname | Permissions.EmbedLinks | 
                                     Permissions.CreatePublicThreads | Permissions.ReadMessageHistory |
                                     Permissions.SendMessagesInThreads;
        
        string minLink = context.Client.CurrentApplication.GenerateBotCommandOAuth(minPerms);
        string adminLink = context.Client.CurrentApplication.GenerateBotCommandOAuth(Permissions.All);

        IEnumerable<DiscordActionRowComponent> buttonRow = new[]
        {
            new DiscordActionRowComponent(new[]
            {
                new DiscordLinkButtonComponent(minLink, "Minimum Permission Invite"),
            }),
            new DiscordActionRowComponent(new []
            {
                new DiscordLinkButtonComponent(adminLink, "Admin Permission Invite - Required for Moderation"),
            }),
        };

        DiscordInteractionResponseBuilder response = new DiscordInteractionResponseBuilder()
            .AsEphemeral(true)
            .WithContent("You can invite me with the links below! They will also give me permission to create slash commands in that server.")
            .AddComponents(buttonRow);
        
        await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, response);
    }

    [SlashCommand("hi", "A basic \"Hello, World!\" slash command for D#+ Slash Commands")]
    public async Task HiCommand(InteractionContext context)
    {
        DiscordInteractionResponseBuilder response = new DiscordInteractionResponseBuilder()
            .AsEphemeral(true)
            .WithContent($":wave: Hi, {context.User.Mention}!")
            .AddMentions(Mentions.None);
        
        await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, response);
    }

    // TODO: Finish after SlashCommands Contexts and Commands can be passed on
    // public async Task ProxyCommand(InteractionContext context, DiscordUser user, string command, string parameters)
    // {
    //     ulong? guildId = this._isDevelopment ? this._normConfig.DevGuildId : null;
    //     DiscordApplicationCommand? foundCommand = context.SlashCommandsExtension.RegisteredCommands
    //         .FirstOrDefault(kvp => kvp.Key == guildId)
    //         .Value.FirstOrDefault(c => c.Name == command);
    //     if (foundCommand is null)
    //         throw new CommandNotFoundException(command);
    //
    //     context.SlashCommandsExtension;
    // }
}