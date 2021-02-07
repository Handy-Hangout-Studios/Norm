using Norm.Configuration;
using Norm.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using Npgsql;
using Norm.Database.EntityTypeConfigurations;

namespace Norm.Database.Contexts
{
    public class NormDbContext : DbContext, IDbContext
    {
        public NormDbContext(IOptions<BotOptions> options, ILoggerFactory factory)
        {
            this.DbConnectionString = options.Value.Database.AsNpgsqlConnectionString();
            this.LoggerFactory = factory;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseNpgsql(this.DbConnectionString, o => o.UseNodaTime())
                .UseLoggerFactory(this.LoggerFactory);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            new GuildBackgroundJobETC().Configure(builder.Entity<GuildBackgroundJob>());
            new GuildEventETC().Configure(builder.Entity<GuildEvent>());
            new GuildLogsChannelETC().Configure(builder.Entity<GuildLogChannel>());
            new GuildModerationAuditRecordETC().Configure(builder.Entity<GuildModerationAuditRecord>());
            new GuildNovelRegistrationETC().Configure(builder.Entity<GuildNovelRegistration>());
            new GuildPrefixETC().Configure(builder.Entity<GuildPrefix>());
            new NovelInfoETC().Configure(builder.Entity<NovelInfo>());
            new UserTimeZoneETC().Configure(builder.Entity<UserTimeZone>());
            base.OnModelCreating(builder);
        }

        public DbContext Context { get => this; }

        private string DbConnectionString { get; }
        public ILoggerFactory LoggerFactory { get; }
        public DbSet<GuildNovelRegistration> GuildNovelRegistrations { get; private set; }
        public DbSet<NovelInfo> AllNovelInfo { get; private set; }
        public DbSet<GuildEvent> GuildEvents { get; private set; }
        public DbSet<GuildPrefix> GuildPrefixes { get; private set; }
        public DbSet<GuildBackgroundJob> GuildBackgroundJobs { get; private set; }
        public DbSet<GuildLogChannel> GuildLogChannels { get; private set; }
        public DbSet<GuildModerationAuditRecord> GuildModerationAuditRecords { get; private set; }
        public DbSet<UserTimeZone> UserTimeZones { get; private set; }
    }
}
