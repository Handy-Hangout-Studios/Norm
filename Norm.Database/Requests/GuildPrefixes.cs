using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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
    public class GuildPrefixes
    {
        public class Add : DbRequest<GuildPrefix>
        {
            public Add(ulong guildId, string prefix)
            {
                this.Prefix = new GuildPrefix(prefix, guildId);
            }

            public GuildPrefix Prefix { get; }
        }

        public class AddHandler : DbRequestHandler<Add, GuildPrefix>
        {
            public AddHandler(NormDbContext context) : base(context) { }

            public override async Task<DbResult<GuildPrefix>> Handle(Add request, CancellationToken cancellationToken)
            {
                EntityEntry<GuildPrefix> entity = this.DbContext.GuildPrefixes.Add(request.Prefix);
                DbResult<GuildPrefix> result = new()
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
            public Delete(GuildPrefix prefix)
            {
                this.Prefix = prefix;
            }

            public GuildPrefix Prefix { get; }
        }

        public class DeleteHandler : DbRequestHandler<Delete>
        {
            public DeleteHandler(NormDbContext context) : base(context) { }

            public override async Task<DbResult> Handle(Delete request, CancellationToken cancellationToken)
            {
                EntityEntry<GuildPrefix> entity = this.DbContext.GuildPrefixes.Remove(request.Prefix);
                DbResult result = new()
                {
                    Success = entity.State.Equals(EntityState.Deleted),
                };
                await this.DbContext.SaveChangesAsync(cancellationToken);
                return result;
            }
        }

        public class GetGuildsPrefixes : DbRequest<IEnumerable<GuildPrefix>>
        {
            public GetGuildsPrefixes(DiscordGuild guild) : this(guild.Id) { }

            public GetGuildsPrefixes(ulong guildId)
            {
                this.GuildId = guildId;
            }

            public ulong GuildId { get; }
        }

        public class GetGuildsPrefixesHandler : DbRequestHandler<GetGuildsPrefixes, IEnumerable<GuildPrefix>>
        {
            public GetGuildsPrefixesHandler(NormDbContext context) : base(context) { }

            public override async Task<DbResult<IEnumerable<GuildPrefix>>> Handle(GetGuildsPrefixes request, CancellationToken cancellationToken)
            {
                try
                {
                    List<GuildPrefix> result =
                        await this.DbContext.GuildPrefixes
                        .Where(p => p.GuildId == request.GuildId)
                        .OrderBy(p => p.Prefix.Length)
                        .ToListAsync(cancellationToken: cancellationToken);

                    return new DbResult<IEnumerable<GuildPrefix>>
                    {
                        Success = true,
                        Value = result,
                    };
                }
                catch (Exception e) when (e is ArgumentNullException or OperationCanceledException)
                {
                    return new DbResult<IEnumerable<GuildPrefix>>
                    {
                        Success = false,
                        Value = null,
                    };
                }
            }
        }
    }
}
