namespace Norm.Database.Entities
{
    public class GuildWelcomeMessageSettings
    {
        public ulong GuildId { get; set; }
        public bool ShouldWelcomeMembers { get; set; }
        public bool ShouldPing { get; set; }
    }
}
