using DSharpPlus;
using System.Threading.Tasks;

namespace Harold.Services
{
    public interface IBotService
    {
        public DiscordShardedClient ShardedClient { get; }
        public bool Started { get; }
        public Task StartAsync();

        public Task StopAsync();
    }
}