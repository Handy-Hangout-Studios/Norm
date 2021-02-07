using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;

namespace Norm.Services
{
    public interface IBotService
    {
        public DiscordShardedClient ShardedClient { get; }
        public DiscordMember BotDeveloper { get; }

        public bool Started { get; }
        public Task StartAsync();

        public Task StopAsync();

        public IMemoryCache PrefixCache { get; }
    }
}