using Harold.Configuration;
using Harold.Database.Entities;
using Harold.Database.EntityConfigurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harold.Database
{
    public class BotPsqlContext : DbContext
    {
        public BotPsqlContext(IOptions<BotConfig> options)
        {
            this.DbConnectionString = options.Value.Database.AsNpgsqlConnectionString();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(this.DbConnectionString);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            new GuildNovelRegistrationEntityTypeConfiguration()
                .Configure(builder.Entity<GuildNovelRegistration>());
            new NovelInfoEntityTypeConfiguration()
                .Configure(builder.Entity<NovelInfo>());
            base.OnModelCreating(builder);
        }

        private string DbConnectionString { get; set; }

        public DbSet<GuildNovelRegistration> GuildNovelRegistrations { get; set; }
        public DbSet<NovelInfo> AllNovelInfo { get; set; }
    }
}
