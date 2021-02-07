using NodaTime;

namespace Norm.Database.Entities
{
    public class GuildBackgroundJob
    {
        public string HangfireJobId { get; set; }

        public ulong GuildId { get; set; }

        public string JobName { get; set; }

        public Instant ScheduledTime { get; set; }

        public GuildJobType GuildJobType { get; set; }
    }

    public enum GuildJobType
    {
        SCHEDULED_EVENT,
        TEMP_BAN,
        TEMP_MUTE,
    }
}
