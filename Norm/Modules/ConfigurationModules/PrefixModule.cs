using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using MediatR;
using Norm.Attributes;
using Norm.Database.Entities;
using Norm.Database.Requests;
using Norm.Services;
using Norm.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Norm.Modules
{
    [Group("prefix")]
    [BotCategory(BotCategory.ConfigAndInfo)]
    [Description("All of my functionalities associated with prefixes.\n\nWhen used alone, show all guild's prefixes separated by spaces")]
    public class PrefixModule : BaseCommandModule
    {
        private readonly IMediator mediator;
        private readonly IBotService bot;

        public PrefixModule(IMediator mediator, IBotService bot)
        {
            this.mediator = mediator;
            this.bot = bot;
        }

        [GroupCommand]
        public async Task ExecuteGroupAsync(CommandContext context)
        {
            string prefixString;
            if (context.Channel.Guild != null)
            {
                List<GuildPrefix> guildPrefixes = (await this.mediator.Send(new GuildPrefixes.GetGuildsPrefixes(context.Guild))).Value.ToList();
                if (!guildPrefixes.Any())
                {
                    prefixString = "My prefix is `^`";
                }
                else
                {
                    prefixString = $"My prefixes are: {string.Join(", ", guildPrefixes.Select(p => Formatter.InlineCode(p.Prefix)))}";
                }
            }
            else
            {
                prefixString = $"You don't need to use a prefix here but you can always start with `^` if you'd like";
            }
            DiscordMessageBuilder msg = new DiscordMessageBuilder()
                .WithContent(prefixString)
                .WithReply(context.Message.Id, mention: true);
            await context.RespondAsync(msg);
        }

        [Command("add"), Description("Add prefix to guild's prefixes\n\nRequirements:\n1. Must be at least one character and at most twenty characters.\n2. You can't use the prefix \",\" as that is used as a separator ")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        [RequireGuild]
        public async Task AddPrefix(
            CommandContext context,
            [Description("The new prefix that you want to add to the guild's prefixes. Must be at least one character")]
            [RemainingText]
            string newPrefix)
        {
            IEnumerable<GuildPrefix> definedPrefixes = (await this.mediator.Send(new GuildPrefixes.GetGuildsPrefixes(context.Guild)))
                .Value;
            if (definedPrefixes.Count() >= 5)
            {
                await context.RespondAsync("I'm sorry, but guilds are only allowed to have a maximum of five custom defined prefixes");
                return;
            }

            if (definedPrefixes.Any(p => p.Prefix.Equals(newPrefix)))
            {
                await context.RespondAsync("That prefix is already in your guild's custom defined prefixes.");
                return;
            }

            if (newPrefix.Length < 1)
            {
                await context.RespondAsync("I'm sorry, but any new prefix must be at least one character.");
                return;
            }

            if (newPrefix.Length > 20)
            {
                await context.RespondAsync("I'm sorry, but any new prefix must be less than 20 characters.");
                return;
            }

            if (newPrefix.Equals(","))
            {
                await context.RespondAsync("I'm sorry, but you can't use the prefix \",\"");
                return;
            }

            await this.mediator.Send(new GuildPrefixes.Add(context.Guild.Id, newPrefix));
            await context.RespondAsync(
                $"Congratulations, you have added the prefix {Formatter.InlineCode(Formatter.Sanitize(newPrefix))} to your server's prefixes for Handy Hansel.\nJust a reminder, this disables the default prefix for Handy Hansel unless you specifically add that prefix in again later or do not have any prefixes of your own.");

            this.PurgeCache(context.Guild);
        }

        [Command("remove")]
        [Description("Remove a prefix from the guild's prefixes")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        [RequireGuild]
        public async Task RemovePrefix(
            CommandContext context,
            [Description("The specific string prefix to remove from the guild's prefixes.")]
            string prefixToRemove)
        {
            GuildPrefix guildPrefix = (await this.mediator.Send(new GuildPrefixes.GetGuildsPrefixes(context.Guild)))
                .Value
                .FirstOrDefault(e => e.Prefix.Equals(prefixToRemove));
            if (guildPrefix is null)
            {
                await context.RespondAsync(
                    $"{context.User.Mention}, I'm sorry but the prefix you have given me does not exist for this guild.");
                return;
            }

            await this.mediator.Send(new GuildPrefixes.Delete(guildPrefix));
            await context.RespondAsync(
                $"{context.User.Mention}, I have removed the prefix {Formatter.InlineCode(Formatter.Sanitize(guildPrefix.Prefix))} for this server.");

            this.PurgeCache(context.Guild);
        }

        [Command("iremove")]
        [Description("Starts an interactive removal process allowing you to choose which prefix to remove")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        [RequireGuild]
        public async Task InteractiveRemovePrefix(CommandContext context)
        {

            List<GuildPrefix> guildPrefixes = (await this.mediator.Send(new GuildPrefixes.GetGuildsPrefixes(context.Guild))).Value.ToList();
            if (!guildPrefixes.Any())
            {
                await context.RespondAsync("You don't have any custom prefixes to remove");
                return;
            }

            DiscordMessage msg = await context.RespondAsync(
                $":wave: Hi, {context.User.Mention}! You want to remove a prefix from your guild list?");
            await msg.CreateReactionAsync(DiscordEmoji.FromName(context.Client, ":regional_indicator_y:"));
            await msg.CreateReactionAsync(DiscordEmoji.FromName(context.Client, ":regional_indicator_n:"));
            InteractivityExtension interactivity = context.Client.GetInteractivity();

            InteractivityResult<MessageReactionAddEventArgs> interactivityResult =
                await interactivity.WaitForReactionAsync(msg, context.User);

            if (interactivityResult.TimedOut ||
                !interactivityResult.Result.Emoji.Equals(
                    DiscordEmoji.FromName(context.Client, ":regional_indicator_y:")))
            {
                DiscordMessage snark = await context.RespondAsync("Well then why did you get my attention! Thanks for wasting my time.");
                return;
            }

            DiscordEmbedBuilder removeEventEmbed = new DiscordEmbedBuilder()
                .WithTitle("Select a prefix to remove by typing: <prefix number>")
                .WithColor(context.Member.Color);

            Task<(bool, int)> messageValidationAndReturn(MessageCreateEventArgs messageE)
            {
                if (messageE.Author.Equals(context.User) && int.TryParse(messageE.Message.Content, out int eventToChoose))
                {
                    return Task.FromResult((true, eventToChoose));
                }
                else
                {
                    return Task.FromResult((false, -1));
                }
            }
            CustomResult<int> result = await context.WaitForMessageAndPaginateOnMsg(
                GetGuildPrefixPages(guildPrefixes, interactivity, removeEventEmbed),
                messageValidationAndReturn,
                msg: msg);

            if (result.TimedOut || result.Cancelled)
            {
                DiscordMessage snark = await context.RespondAsync("You never gave me a valid input. Thanks for wasting my time. :triumph:");
                return;
            }

            GuildPrefix selectedPrefix = guildPrefixes[result.Result - 1];

            await this.mediator.Send(new GuildPrefixes.Delete(selectedPrefix));

            await context.RespondAsync(
                $"You have deleted the prefix {Formatter.InlineCode(Formatter.Sanitize(selectedPrefix.Prefix))} from this guild's prefixes.", embed: null);

            this.PurgeCache(context.Guild);
        }

        private void PurgeCache(DiscordGuild guild)
        {
            this.bot.PrefixCache.Remove(guild.Id);
        }

        private static IEnumerable<Page> GetGuildPrefixPages(List<GuildPrefix> guildPrefixes, InteractivityExtension interactivity, DiscordEmbedBuilder pageEmbedBase = null)
        {
            StringBuilder guildPrefixesStringBuilder = new();
            int count = 1;
            foreach (GuildPrefix prefix in guildPrefixes)
            {
                guildPrefixesStringBuilder.AppendLine($"{count}. {prefix.Prefix}");
                count++;
            }

            return interactivity.GeneratePagesInEmbed(guildPrefixesStringBuilder.ToString(), SplitType.Line, pageEmbedBase);
        }
    }
}
