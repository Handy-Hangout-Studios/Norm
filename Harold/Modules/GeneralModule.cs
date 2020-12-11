using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Harold.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harold.Modules
{
    public class GeneralModule : BaseCommandModule
    {
        private readonly string inviteLink;

        public GeneralModule(IOptions<BotConfig> options)
        {
            this.inviteLink = options.Value.InviteLink;
        }

        [Command("invite")]
        [Description("Generate an invite link for this bot")]
        public async Task InviteAsync(CommandContext context)
        {
            await context.RespondAsync(embed: new DiscordEmbedBuilder().WithDescription($"[Invite]({this.inviteLink})"));
        }
    }
}
