namespace Norm.Database.Entities
{
    public class GuildPrefix
    {
        public int Id { get; set; }
        public string Prefix { get; set; }
        public ulong GuildId { get; set; }
    }
}