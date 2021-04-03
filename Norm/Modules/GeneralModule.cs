using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Norm.Attributes;
using Owoify;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Norm.Modules
{
    public class GeneralModule : BaseCommandModule
    {
        [Command("invite")]
        [Description("Generate an invite link for this bot")]
        [BotCategory(BotCategory.General)]
        public async Task InviteAsync(CommandContext context)
        {
            DiscordEmbed embed = new DiscordEmbedBuilder()
                .WithTitle("You want me in your server!")
                .WithDescription("Well, there are three level's of permission you can possibly give me and thus set what I can do. " +
                "First, is level 1 where I can act as an automatic timezone converter for every kind of time I recognizes. " +
                "Second, is level 2 where I can do everything from level 1 but also act as a tool for scheduling events and making announcements about say... your favorite webnovel from RoyalRoad. " +
                "Third, is level 3 where I act as a moderation assistance tool but also can do everything from level 2 and 1. (Fair warning, Level 3 means giving the bot administrator permissions)\n\n" +
                "Make your choice below:")
                .AddField("Level 1", $"[Click this link]({context.Client.CurrentApplication.GenerateBotOAuth(Level1)})")
                .AddField("Level 2", $"[Click this link]({context.Client.CurrentApplication.GenerateBotOAuth(Level2)})")
                .AddField("Level 3", $"[Click this link]({context.Client.CurrentApplication.GenerateBotOAuth(Level3)})");

            await context.RespondAsync(embed: embed);
        }

        [Command("hi")]
        [Description("A basic \"Hello, World!\" command for D#+")]
        [BotCategory(BotCategory.General)]
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

        [Command("tutorial")]
        [Description("A general high-level overview of my functionalites will be DM-ed to you.")]
        [BotCategory(BotCategory.General)]
        public async Task Tutorial(CommandContext context)
        {
            DiscordEmbed tutorial = new DiscordEmbedBuilder()
                .WithTitle("Tutorial")
                .WithDescription($"Hi, my name is {context.Client.CurrentUser.Username}! I'm a useful bot with multiple features. If you'd like to see all of them you can do `help` in DMs or `@Norm help`.\n\nYou can see all my most directly useful or hardest to find features below. :smile:")
                .AddField("Time Zone Conversions", "If you react to any message that has a time in it with the :clock: emoji, I will DM you a time zone conversion from their time to your time. Of course, this does require that both you and they have your timezone set up with me. You can do so by saying `time init` here in the DMs or `@Norm time init` in any server.")
                .AddField("Custom Prefixes", "Any server can set up to 5 custom prefixes for their server, you can see your server's prefixes by typing `@Norm prefix` in your server.\n\nTo see the requirements for the prefixes use `help prefix add` or `@Norm help prefix add`.")
                .AddField("There's More!", "To see all the rest of my features, just type `help` in the DM or `@Norm help` in any server I'm in.")
                .WithFooter("Note: Any place where it says you can say `@Norm <command>` you can replace `@Norm` with any of your server's prefixes.")
                .WithColor(DiscordColor.Cyan);

            if (context.Channel.Guild == null)
            {
                await context.RespondAsync(tutorial);
            }
            else
            {
                await context.Member.SendMessageAsync(tutorial);
            }
        }

        [Command("break")]
        [Description("Purposefully throw an error for testing purposes")]
        [RequireOwner]
        [BotCategory(BotCategory.General)]
#pragma warning disable CA1822 // Mark members as static
        public async Task Break(CommandContext context)
#pragma warning restore CA1822 // Mark members as static
        {
            await context.RespondAsync("Throwing an exception now");
            throw new Exception();
        }

        [Command("proxy")]
        [Description("Execute Command as another user")]
        [RequireOwner]
        public async Task MaskAsync(CommandContext context, DiscordUser user, string command, [RemainingText] string parameters)
        {
            if (!context.CommandsNext.RegisteredCommands.TryGetValue(command, out Command cmd))
            {
                throw new CommandNotFoundException("command");
            }
            
            
            await context.RespondAsync($"Executing the command `{command}` on behalf of {context.Member.Mention} as {user.Mention} with the input `{parameters}`");
            CommandContext innerContext = context.CommandsNext.CreateFakeContext(user, context.Channel, context.Message.Content, "", cmd, parameters);
            await context.CommandsNext.ExecuteCommandAsync(innerContext).ConfigureAwait(false);
        }

        private const Permissions Level1 = Permissions.AddReactions | Permissions.ChangeNickname | Permissions.ReadMessageHistory | Permissions.SendMessages | Permissions.AccessChannels | Permissions.EmbedLinks;
        private const Permissions Level2 = Level1 | Permissions.MentionEveryone;
        private const Permissions Level3 = Permissions.Administrator;
    }
}
