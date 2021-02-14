using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Norm.Database.Contexts;
using Norm.Database.Entities;
using Norm.Database.Requests.BaseClasses;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Norm.Database.Requests
{
    public class GuildEvents
    {
        public class Add : DbRequest<GuildEvent>
        {
            public Add(ulong guildId, string eventName, string eventDesc)
            {
                this.Event = new GuildEvent { GuildId = guildId, EventName = eventName, EventDesc = eventDesc };
            }

            public GuildEvent Event { get; }
        }

        public class AddHandler : DbRequestHandler<Add, GuildEvent>
        {
            public AddHandler(IDbContext db) : base(db) { }
            public override async Task<DbResult<GuildEvent>> Handle(Add request, CancellationToken cancellationToken)
            {
                EntityEntry<GuildEvent> entity = this.DbContext.GuildEvents.Add(request.Event);
                DbResult<GuildEvent> result = new DbResult<GuildEvent>
                {
                    Success = entity.State.Equals(EntityState.Added),
                    Value = entity.Entity,
                };
                await this.DbContext.Context.SaveChangesAsync(cancellationToken);

                return result;
            }
        }

        public class Delete : DbRequest
        {
            public Delete(GuildEvent e)
            {
                this.Event = e;
            }

            public GuildEvent Event { get; }
        }

        public class DeleteHandler : DbRequestHandler<Delete>
        {
            public DeleteHandler(IDbContext dbContext) : base(dbContext) { }

            public override async Task<DbResult> Handle(Delete request, CancellationToken cancellationToken)
            {
                EntityEntry<GuildEvent> entity = this.DbContext.GuildEvents.Remove(request.Event);
                DbResult result = new DbResult
                {
                    Success = entity.State.Equals(EntityState.Deleted),
                };
                await this.DbContext.Context.SaveChangesAsync(cancellationToken);
                return result;
            }
        }

        public class GetGuildEvents : DbRequest<IEnumerable<GuildEvent>>
        {
            public GetGuildEvents(DiscordGuild guild) : this(guild.Id) { }

            public GetGuildEvents(ulong guildId)
            {
                this.GuildId = guildId;
            }

            public ulong GuildId { get; }
        }

        public class GetGuildEventsHandler : DbRequestHandler<GetGuildEvents, IEnumerable<GuildEvent>>
        {
            public GetGuildEventsHandler(IDbContext dbContext) : base(dbContext) { }

            public override async Task<DbResult<IEnumerable<GuildEvent>>> Handle(GetGuildEvents request, CancellationToken cancellationToken)
            {
                List<GuildEvent> result = await this.DbContext.GuildEvents
                    .Where(x => x.GuildId == request.GuildId)
                    .ToListAsync(cancellationToken: cancellationToken);

                return new DbResult<IEnumerable<GuildEvent>>
                {
                    Success = true,
                    Value = result,
                };
            }
        }
    }
}
