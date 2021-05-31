using NodaTime;

namespace Norm.Database.Entities
{
    public class GuildModerationAuditRecord
    {
        public GuildModerationAuditRecord(
            ulong guildId,
            ulong moderatorUserId,
            ulong userId,
            ModerationActionType moderationAction,
            string? reason)
        {
            this.GuildId = guildId;
            this.ModeratorUserId = moderatorUserId;
            this.UserId = userId;
            this.ModerationAction = moderationAction;
            this.Reason = reason;
        }

        public int Id { get; set; }

        public ulong GuildId { get; set; }

        public ulong ModeratorUserId { get; set; }

        public ulong UserId { get; set; }

        public ModerationActionType ModerationAction { get; set; }

        public string? Reason { get; set; }

        public Instant? Timestamp { get; set; }
    }

    public enum ModerationActionType
    {
        NONE,
        WARN,
        BAN,
        TEMPBAN,
        MUTE,
        TEMPMUTE,
        KICK,
    }
}