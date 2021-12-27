using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Norm.Database.Entities;
using Norm.Database.Requests;
using System.Threading.Tasks;
using Norm.Database.Requests.BaseClasses;

namespace Norm.Services
{
    public partial class BotService
    {
        private async Task SendWelcomeMessage(DiscordClient sender, GuildMemberAddEventArgs e)
        {
            if (e.Member.IsBot)
                return;

            DbResult<GuildWelcomeMessageSettings> result = await this.Mediator.Send(new GuildWelcomeMessageSettingsRequest.GetGuildWelcomeMessageSettings(e.Guild));
            if (!result.TryGetValue(out GuildWelcomeMessageSettings? settings) || !settings.ShouldWelcomeMembers)
                return;

            DiscordMember bot = await e.Guild.GetMemberAsync(sender.CurrentUser.Id);
            await e.Guild.SystemChannel.SendMessageAsync($"Hi {(settings.ShouldPing ? e.Member.Mention : e.Member.DisplayName)}, I'm {bot.Mention}! Glad that you are here! If you'd like learn more about me, you can do `@{bot.DisplayName} tutorial` or DM me `tutorial` and I'll send you a DM with more info.");
        }
    }
}
