namespace Norm.Database.Entities
{
    public class GuildLogChannel
    {
        public int Id { get; set; }

        public ulong GuildId { get; set; }

        public ulong ChannelId { get; set; }
    }
}
