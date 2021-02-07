using NodaTime;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Norm.Database.Entities
{
    public class GuildModerationAuditRecord
    {
        public int Id { get; set; }

        public ulong GuildId { get; set; }

        public ulong ModeratorUserId { get; set; }

        public ulong UserId { get; set; }

        public ModerationActionType ModerationAction { get; set; }

        public string Reason { get; set; }

        public Instant Timestamp { get; set; }
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
        NEGATEKARMA,
    }
}