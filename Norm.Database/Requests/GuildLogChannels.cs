using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Norm.Database.Contexts;
using Norm.Database.Entities;
using Norm.Database.Requests.BaseClasses;
using System.Threading;
using System.Threading.Tasks;

namespace Norm.Database.Requests
{
    public class GuildLogChannels
    {
        public class Upsert : DbRequest<GuildLogChannel>
        {
            public Upsert(ulong guildId, ulong channelId)
            {
                this.LogChannel = new GuildLogChannel(guildId, channelId);
            }

            public GuildLogChannel LogChannel { get; }
        }

        public class UpsertHandler : DbRequestHandler<Upsert, GuildLogChannel>
        {
            public UpsertHandler(NormDbContext db) : base(db) { }
            public override async Task<DbResult<GuildLogChannel>> Handle(Upsert request, CancellationToken cancellationToken)
            {
                GuildLogChannel? logChannel = await this.DbContext.GuildLogChannels.FirstOrDefaultAsync(channel => channel.GuildId == request.LogChannel.GuildId, cancellationToken: cancellationToken);
                EntityEntry<GuildLogChannel> entity;
                DbResult<GuildLogChannel> result;
                if (logChannel is not null)
                {
                    logChannel.ChannelId = request.LogChannel.ChannelId;
                    entity = this.DbContext.GuildLogChannels.Update(logChannel);
                    result = new DbResult<GuildLogChannel>
                    {
                        Success = entity.Entity != null && entity.State.Equals(EntityState.Modified),
                        Value = entity.Entity,
                    };
                }
                else
                {
                    entity = this.DbContext.GuildLogChannels.Add(request.LogChannel);
                    result = new DbResult<GuildLogChannel>
                    {
                        Success = entity.Entity != null && entity.State.Equals(EntityState.Added),
                        Value = entity.Entity,
                    };
                }

                await this.DbContext.SaveChangesAsync(cancellationToken);

                return result;
            }
        }

        public class Delete : DbRequest
        {
            public Delete(DiscordGuild guild) : this(guild.Id) { }

            public Delete(ulong guildId)
            {
                this.GuildId = guildId;
            }

            public ulong GuildId { get; }
        }

        public class DeleteHandler : DbRequestHandler<Delete>
        {
            public DeleteHandler(NormDbContext dbContext) : base(dbContext) { }

            public override async Task<DbResult> Handle(Delete request, CancellationToken cancellationToken)
            {
                GuildLogChannel? logChannel = await this.DbContext.GuildLogChannels.FirstOrDefaultAsync(channel => channel.GuildId == request.GuildId, cancellationToken: cancellationToken);
                if (logChannel == null)
                {
                    return new DbResult
                    {
                        Success = true,
                    };
                }

                EntityEntry<GuildLogChannel> entity = this.DbContext.GuildLogChannels.Remove(logChannel);
                DbResult result = new()
                {
                    Success = entity.State.Equals(EntityState.Deleted),
                };
                await this.DbContext.SaveChangesAsync(cancellationToken);
                return result;
            }
        }

        public class GetGuildLogChannel : DbRequest<GuildLogChannel>
        {
            public GetGuildLogChannel(DiscordGuild guild) : this(guild.Id) { }

            public GetGuildLogChannel(ulong guildId)
            {
                this.GuildId = guildId;
            }

            public ulong GuildId { get; }
        }

        public class GetGuildEventsHandler : DbRequestHandler<GetGuildLogChannel, GuildLogChannel>
        {
            public GetGuildEventsHandler(NormDbContext dbContext) : base(dbContext) { }

            public override async Task<DbResult<GuildLogChannel>> Handle(GetGuildLogChannel request, CancellationToken cancellationToken)
            {
                GuildLogChannel? result = await this.DbContext.GuildLogChannels
                    .FirstOrDefaultAsync(channel => channel.GuildId == request.GuildId, cancellationToken: cancellationToken);
                return new DbResult<GuildLogChannel>
                {
                    Success = result != null,
                    Value = result,
                };
            }
        }
    }
}
