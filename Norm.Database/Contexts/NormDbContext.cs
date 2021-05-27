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

        public DbContext Context => this;

        // Novel Tables
        public DbSet<NovelInfo> AllNovelInfo { get; private set; }
        public DbSet<GuildNovelRegistration> GuildNovelRegistrations { get; private set; }

        // Guild Tables
        public DbSet<GuildEvent> GuildEvents { get; private set; }
        public DbSet<GuildPrefix> GuildPrefixes { get; private set; }
        public DbSet<GuildBackgroundJob> GuildBackgroundJobs { get; private set; }
        public DbSet<GuildLogChannel> GuildLogChannels { get; private set; }
        public DbSet<GuildModerationAuditRecord> GuildModerationAuditRecords { get; private set; }
        public DbSet<GuildWelcomeMessageSettings> GuildWelcomeMessages { get; private set; }
        public DbSet<GuildMovieNight> GuildMovieNights { get; private set; }
        public DbSet<GuildMovieSuggestion> GuildMovieSuggestions { get; private set; }
        
        // User Tables
        public DbSet<UserTimeZone> UserTimeZones { get; private set; }
    }
}
