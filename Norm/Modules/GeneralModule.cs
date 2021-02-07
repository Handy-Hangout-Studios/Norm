using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Norm.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Norm.Attributes;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus;

namespace Norm.Modules
{
    [BotCategory("General")]
    public class GeneralModule : BaseCommandModule
    {
        [Command("invite")]
        [Description("Generate an invite link for this bot")]
        public async Task InviteAsync(CommandContext context)
        {
            DiscordEmbed embed = new DiscordEmbedBuilder()
                .WithTitle("So you want Norm in your server?")
                .WithDescription("Well, there are three level's of permission you can possibly give Norm and thus set what he can do. " +
                "First, is level 1 where Norm acts as an automatic timezone converter for every kind of time he recognizes. " +
                "Second, is level 2 where Norm can do everything from level 1 but also acts as a tool for scheduling events and making announcements about say... your favorite webnovel from RoyalRoad. " +
                "Third, is level 3 where Norm acts as a moderation assistance tool but also does everything from level 2 and 1. (Fair warning, Level 3 means giving the bot administrator permissions)\n\n" +
                "Make your choice below:")
                .AddField("Level 1", $"[Click this link]({context.Client.CurrentApplication.GenerateBotOAuth(Level1)})")
                .AddField("Level 2", $"[Click this link]({context.Client.CurrentApplication.GenerateBotOAuth(Level2)})")
                .AddField("Level 3", $"[Click this link]({context.Client.CurrentApplication.GenerateBotOAuth(Level3)})");

            await context.RespondAsync(embed: embed);
        }

        [Command("hi")]
        [Description("A basic \"Hello, World!\" command for D#+")]
#pragma warning disable CA1822 // Mark members as static
        public async Task Hi(CommandContext context)
#pragma warning restore CA1822 // Mark members as static
        {
            await context.RespondAsync($":wave: Hi, {context.User.Mention}!");
            InteractivityExtension interactivity = context.Client.GetInteractivity();
            InteractivityResult<DiscordMessage> result = await interactivity.WaitForMessageAsync(
                xm => xm.Author.Id == context.User.Id && xm.Content.ToLower() == "how are you?",
                TimeSpan.FromMinutes(1));
            if (!result.TimedOut)
            {
                await context.RespondAsync("I'm fine, thank you!");
            }
        }

        [Command("break")]
        [Description("Purposefully throw an error for testing purposes")]
        [RequireOwner]
        [Hidden]
        [BotCategory("General")]
#pragma warning disable CA1822 // Mark members as static
        public async Task Break(CommandContext context)
#pragma warning restore CA1822 // Mark members as static
        {
            await context.RespondAsync("Throwing an exception now");
            throw new Exception();
        }

        private const Permissions Level1 = Permissions.AddReactions | Permissions.ChangeNickname | Permissions.ReadMessageHistory | Permissions.SendMessages | Permissions.AccessChannels | Permissions.EmbedLinks;
        private const Permissions Level2 = Level1 | Permissions.MentionEveryone;
        private const Permissions Level3 = Permissions.Administrator;
    }
}
