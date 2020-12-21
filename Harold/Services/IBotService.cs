using DSharpPlus;
using DSharpPlus.Entities;
using System.Threading.Tasks;

namespace Harold.Services
{
    public interface IBotService
    {
        public DiscordShardedClient ShardedClient { get; }
        public DiscordMember BotDeveloper { get; }

        public bool Started { get; }
        public Task StartAsync();

        public Task StopAsync();
    }
}