using Microsoft.EntityFrameworkCore;
using Norm.Database.Entities;

namespace Norm.Database.Contexts
{
    public class NormDbContext : DbContext
    {
        public NormDbContext(DbContextOptions options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfigurationsFromAssembly(typeof(NormDbContext).Assembly);
            base.OnModelCreating(builder);
        }

        // Novel Tables
        public DbSet<NovelInfo> AllNovelInfo => Set<NovelInfo>();
        public DbSet<GuildNovelRegistration> GuildNovelRegistrations => Set<GuildNovelRegistration>();

        // Guild Tables
        public DbSet<GuildEvent> GuildEvents => Set<GuildEvent>();
        public DbSet<GuildPrefix> GuildPrefixes => Set<GuildPrefix>();
        public DbSet<GuildBackgroundJob> GuildBackgroundJobs => Set<GuildBackgroundJob>();
        public DbSet<GuildLogChannel> GuildLogChannels => Set<GuildLogChannel>();
        public DbSet<GuildModerationAuditRecord> GuildModerationAuditRecords => Set<GuildModerationAuditRecord>();
        public DbSet<GuildWelcomeMessageSettings> GuildWelcomeMessages => Set<GuildWelcomeMessageSettings>();
        public DbSet<GuildMovieNight> GuildMovieNights => Set<GuildMovieNight>();
        public DbSet<GuildMovieSuggestion> GuildMovieSuggestions => Set<GuildMovieSuggestion>();

        // User Tables
        public DbSet<UserTimeZone> UserTimeZones => Set<UserTimeZone>();
    }
}
