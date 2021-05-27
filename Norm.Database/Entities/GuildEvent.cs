using System.Diagnostics.CodeAnalysis;

namespace Norm.Database.Entities
{
    public class GuildEvent
    {
        public GuildEvent(ulong guildId, string eventName, string eventDesc)
        {
            this.GuildId = guildId;
            this.EventName = eventName;
            this.EventDesc = eventDesc;
        }

        public int Id { get; set; }
        public ulong GuildId { get; set; }
        public string EventName { get; set; }
        public string EventDesc { get; set; }
    }
}