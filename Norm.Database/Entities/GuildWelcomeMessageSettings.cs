namespace Norm.Database.Entities
{
    public class GuildWelcomeMessageSettings
    {
        public GuildWelcomeMessageSettings(ulong guildId, bool shouldWelcomeMembers, bool shouldPing)
        {
            this.GuildId = guildId;
            this.ShouldWelcomeMembers = shouldWelcomeMembers;
            this.ShouldPing = shouldPing;
        }

        public ulong GuildId { get; set; }
        public bool ShouldWelcomeMembers { get; set; }
        public bool ShouldPing { get; set; }
    }
}
