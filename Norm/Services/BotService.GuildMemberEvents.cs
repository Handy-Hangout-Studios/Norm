using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Norm.Services
{
    public partial class BotService
    {
        private async Task SendWelcomeMessage(DiscordClient sender, GuildMemberAddEventArgs e)
        {
            DiscordMember bot = await e.Guild.GetMemberAsync(sender.CurrentUser.Id);
            await e.Guild.SystemChannel.SendMessageAsync($"Hi {e.Member.DisplayName}, I'm {bot.Mention}! Glad that you are here, if you'd like learn more about me, you can do `{bot.Mention} tutorial` or DM me `tutorial` and I'll send you a DM with more info.");
        }
    }
}
