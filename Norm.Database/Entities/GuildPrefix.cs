using System.Diagnostics.CodeAnalysis;

namespace Norm.Database.Entities
{
    public class GuildPrefix
    {
        public GuildPrefix(string prefix, ulong guildId)
        {
            this.Prefix = prefix;
            this.GuildId = guildId;
        }

        public int Id { get; set; }
        public string Prefix { get; set; }
        public ulong GuildId { get; set; }
    }
}