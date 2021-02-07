using Norm.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Norm.Database.Contexts
{
    public interface IDbContext
    {
        public DbContext Context { get; }
        public DbSet<GuildNovelRegistration> GuildNovelRegistrations { get; }
        public DbSet<NovelInfo> AllNovelInfo { get; }
        public DbSet<GuildEvent> GuildEvents { get; }
        public DbSet<GuildPrefix> GuildPrefixes { get; }
        public DbSet<GuildBackgroundJob> GuildBackgroundJobs { get; }
        public DbSet<GuildLogChannel> GuildLogChannels { get; }
        public DbSet<GuildModerationAuditRecord> GuildModerationAuditRecords { get; }
        public DbSet<UserTimeZone> UserTimeZones { get; }
    }
}
