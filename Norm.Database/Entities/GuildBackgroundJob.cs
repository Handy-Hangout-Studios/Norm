using NodaTime;

namespace Norm.Database.Entities
{
    public class GuildBackgroundJob
    {

        public GuildBackgroundJob(
            string hangfireJobId,
            ulong guildId,
            string jobName,
            Instant scheduledTime,
            GuildJobType guildJobType)
        {
            this.HangfireJobId = hangfireJobId;
            this.GuildId = guildId;
            this.JobName = jobName;
            this.ScheduledTime = scheduledTime;
            this.GuildJobType = guildJobType;
        }

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
