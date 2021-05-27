using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Norm.Services
{
    public class ModerationService
    {
        private readonly BotService bot;
        private readonly ILogger logger;
        public ModerationService(BotService bot, ILogger<ModerationService> logger)
        {
            this.bot = bot;
            this.logger = logger;
        }

        public async Task UnbanAsync(ulong guildId, ulong userId)
        {
            try
            {
                DiscordClient shardClient = this.bot.ShardedClient.GetShard(guildId);
                DiscordUser discordUser = await shardClient.GetUserAsync(userId);
                DiscordGuild discordGuild = await shardClient.GetGuildAsync(guildId);
                await discordUser.UnbanAsync(discordGuild);
            }
            catch (Exception e)
            {
                this.logger.LogError("Error in unbanning user", e);
            }
        }

        public async Task RemoveRole(ulong guildId, ulong userId, ulong roleId)
        {
            try
            {
                DiscordClient shardClient = this.bot.ShardedClient.GetShard(guildId);
                DiscordGuild discordGuild = await shardClient.GetGuildAsync(guildId);
                DiscordMember guildMember = await discordGuild.GetMemberAsync(userId);
                DiscordRole guildRole = discordGuild.GetRole(roleId);
                await guildMember.RevokeRoleAsync(guildRole);
            }
            catch (Exception e)
            {
                this.logger.LogError("Error in revoking role", e);
            }
        }
    }
}
