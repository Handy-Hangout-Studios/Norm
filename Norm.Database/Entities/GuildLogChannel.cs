namespace Norm.Database.Entities
{
    public class GuildLogChannel
    {
        public GuildLogChannel(ulong guildId, ulong channelId)
        {
            this.GuildId = guildId;
            this.ChannelId = channelId;
        }

        public int Id { get; set; }

        public ulong GuildId { get; set; }

        public ulong ChannelId { get; set; }
    }
}
