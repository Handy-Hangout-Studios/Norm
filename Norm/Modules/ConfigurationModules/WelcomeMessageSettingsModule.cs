using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using MediatR;
using Norm.Attributes;
using Norm.Database.Entities;
using Norm.Database.Requests;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Norm.Modules
{
    [Group("welcome")]
    [BotCategory(BotCategory.ConfigAndInfo)]
    [Description("All functionalities associated with welcome messages in Norm.\n\nWhen used alone, shows your current welcome message settings for me")]
    [RequireUserPermissions(Permissions.ManageGuild)]
    [Aliases("w", "wel")]
    public class WelcomeMessageSettingsModule : BaseCommandModule
    {
        private readonly IMediator mediator;
        public WelcomeMessageSettingsModule(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [GroupCommand]
        [RequireGuild]
        public async Task ExecuteGroupAsync(CommandContext context)
        {
            DbResult<GuildWelcomeMessageSettings> result = await this.mediator.Send(new GuildWelcomeMessageSettingsRequest.GetGuildWelcomeMessageSettings(context.Guild));
            DiscordMessageBuilder builder = new();
            if (!result.TryGetValue(out GuildWelcomeMessageSettings? settings) || !settings.ShouldWelcomeMembers)
            {
                builder.WithContent("Currently, I won't send a welcome message to new members.");
            }
            else if (settings.ShouldWelcomeMembers)
            {
                builder.WithContent($"Currently, I will send a welcome message to new members {(settings.ShouldPing ? "and ping them in it" : "without pinging them")}");
            }

            builder.WithReply(context.Message.Id, mention: true);
            await context.RespondAsync(builder);
        }

        [Command("update")]
        [Description("Update the welcome message settings for Norm")]
        [Aliases("u")]
        public async Task UpdateGuildWelcomeSettings(CommandContext context,
            [Description("Whether I should send welcome messages to new members or not. (true/false)")]
            bool shouldWelcomeMembers = false,
            [Description("Whether I should ping new members when I send them a welcome message. (true/false)")]
            bool shouldPing = false)
        {
            DbResult<GuildWelcomeMessageSettings> result = await this.mediator.Send(new GuildWelcomeMessageSettingsRequest.Upsert(context.Guild, shouldWelcomeMembers, shouldPing));
            if (!result.TryGetValue(out GuildWelcomeMessageSettings? settings))
            {
                await context.RespondAsync("I failed to update your welcome message settings. A report has been sent to the developer. Please try again later.");
                throw new Exception($"There was an error updating the welcome message settings for {context.Guild.Id}");
            }

            DiscordMessageBuilder builder = new();
            StringBuilder contentBuilder = new StringBuilder().Append("I have successfully updated your welcome message settings. ");



            if (settings.ShouldWelcomeMembers)
            {
                contentBuilder.Append("I will send new members a welcome message ");
                if (settings.ShouldPing)
                {
                    contentBuilder.Append("and ping them in it.");
                }
                else
                {
                    contentBuilder.Append("without pinging them.");
                }
            }
            else
            {
                contentBuilder.Append("I will not send new members a welcome message.");
            }

            builder
                .WithContent(contentBuilder.ToString())
                .WithReply(context.Message.Id, mention: true);

            await context.RespondAsync(builder);
        }

        [Command("deactivate")]
        [Description("Deactivate the welcome message for new members")]
        [Aliases("d")]
        public async Task DeactivateWelcomeMessage(CommandContext context)
        {
            await this.UpdateGuildWelcomeSettings(context, false, false);
        }
    }
}
