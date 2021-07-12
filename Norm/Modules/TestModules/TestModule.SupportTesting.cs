using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Threading.Tasks;

namespace Norm.Modules.TestModules
{
    public partial class TestModule
    {
        [Command("pc")]
        [Aliases("purgechat")]
        [Description("Purges chat")]
        [RequirePermissions(Permissions.ManageChannels)]
        public async Task PurgeChatAsync(CommandContext ctx)
        {
            Serilog.Log.Debug("PC");
            DiscordChannel channel = ctx.Channel;
            var z = ctx.Channel.Position;
            var x = await channel.CloneAsync();
            await channel.DeleteAsync();
            await x.ModifyPositionAsync(z);
            var embed2 = new DiscordEmbedBuilder()
                .WithTitle("✅ Purged")
                .WithFooter($"(C) 𝖆𝖇𝖓𝖔𝖗𝖒𝖆𝖑#0666, foo");
            await x.SendMessageAsync(embed: embed2);
        }
    }
}
