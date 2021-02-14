using Microsoft.EntityFrameworkCore;
using Norm.Database.Entities;

namespace Norm.Database.Contexts
{
    public interface IDbContext
    {
        public DbContext Context { get; }
        public DbSet<NovelInfo> AllNovelInfo { get; }
        public DbSet<GuildNovelRegistration> GuildNovelRegistrations { get; }
        public DbSet<GuildEvent> GuildEvents { get; }
        public DbSet<GuildPrefix> GuildPrefixes { get; }
        public DbSet<GuildBackgroundJob> GuildBackgroundJobs { get; }
        public DbSet<GuildLogChannel> GuildLogChannels { get; }
        public DbSet<GuildModerationAuditRecord> GuildModerationAuditRecords { get; }
        public DbSet<GuildWelcomeMessageSettings> GuildWelcomeMessages { get; }
        public DbSet<UserTimeZone> UserTimeZones { get; }
    }
}
