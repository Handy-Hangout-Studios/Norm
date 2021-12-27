using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using NodaTime;
using Norm.Database.Contexts;
using Norm.Database.Entities;
using Norm.Database.Requests.BaseClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Norm.Database.Requests
{
    public class GuildBackgroundJobs
    {
        public class Add : DbRequest<GuildBackgroundJob>
        {
            public Add(string hangfireJobId, ulong guildId, string jobName, Instant scheduledTime, GuildJobType guildJobType)
            {
                this.Job = new GuildBackgroundJob(hangfireJobId, guildId, jobName, scheduledTime, guildJobType);
            }

            public GuildBackgroundJob Job { get; }
        }

        public class AddHandler : DbRequestHandler<Add, GuildBackgroundJob>
        {
            public AddHandler(NormDbContext db) : base(db) { }
            public override async Task<DbResult<GuildBackgroundJob>> Handle(Add request, CancellationToken cancellationToken)
            {
                EntityEntry<GuildBackgroundJob> entity = this.DbContext.GuildBackgroundJobs.Add(request.Job);
                DbResult<GuildBackgroundJob> result = new()
                {
                    Success = entity.Entity != null && entity.State.Equals(EntityState.Added),
                    Value = entity.Entity,
                };
                await this.DbContext.SaveChangesAsync(cancellationToken);
                return result;
            }
        }

        public class Delete : DbRequest
        {
            public Delete(GuildBackgroundJob job)
            {
                this.Job = job;
            }

            public GuildBackgroundJob Job { get; }
        }

        public class DeleteHandler : DbRequestHandler<Delete>
        {
            public DeleteHandler(NormDbContext dbContext) : base(dbContext) { }

            public override async Task<DbResult> Handle(Delete request, CancellationToken cancellationToken)
            {
                EntityEntry<GuildBackgroundJob> entity = this.DbContext.GuildBackgroundJobs.Remove(request.Job);
                DbResult result = new()
                {
                    Success = entity.State.Equals(EntityState.Deleted),
                };
                await this.DbContext.SaveChangesAsync(cancellationToken);
                return result;
            }
        }

        public class GetGuildJobs : DbRequest<IEnumerable<GuildBackgroundJob>>
        {
            public GetGuildJobs(DiscordGuild guild) : this(guild.Id) { }

            public GetGuildJobs(ulong guildId)
            {
                this.GuildId = guildId;
            }

            public ulong GuildId { get; }
        }

        public class GetGuildJobsHandler : DbRequestHandler<GetGuildJobs, IEnumerable<GuildBackgroundJob>>
        {
            public GetGuildJobsHandler(NormDbContext dbContext, IClock clock) : base(dbContext)
            {
                this.Clock = clock;
            }

            public override async Task<DbResult<IEnumerable<GuildBackgroundJob>>> Handle(GetGuildJobs request, CancellationToken cancellationToken)
            {
                try
                {
                    List<GuildBackgroundJob> result =
                        await this.DbContext.GuildBackgroundJobs
                        .Where(x => x.GuildId == request.GuildId && x.ScheduledTime > this.Clock.GetCurrentInstant())
                        .ToListAsync(cancellationToken: cancellationToken);

                    return new DbResult<IEnumerable<GuildBackgroundJob>>
                    {
                        Success = true,
                        Value = result,
                    };
                }
                catch (Exception e) when (e is ArgumentNullException or OperationCanceledException)
                {
                    return new DbResult<IEnumerable<GuildBackgroundJob>>
                    {
                        Success = false,
                        Value = null,
                    };
                }
            }

            private IClock Clock { get; }
        }
    }
}
