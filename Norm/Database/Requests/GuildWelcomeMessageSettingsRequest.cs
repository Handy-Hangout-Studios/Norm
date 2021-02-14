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
    public class GuildWelcomeMessageSettingsRequest
    {
        public class Upsert : DbRequest<GuildWelcomeMessageSettings>
        {
            public Upsert(DiscordGuild guild, bool shouldWelcomeMembers = false, bool shouldPing = false) : this(guild.Id, shouldWelcomeMembers, shouldPing) { }

            public Upsert(ulong guildId, bool shouldWelcomeMembers = false, bool shouldPing = false)
            {
                this.GuildWelcomeMessage = new GuildWelcomeMessageSettings
                {
                    GuildId = guildId,
                    ShouldWelcomeMembers = shouldWelcomeMembers,
                    ShouldPing = shouldPing,
                };
            }

            public GuildWelcomeMessageSettings GuildWelcomeMessage { get; }
        }

        public class UpsertHandler : DbRequestHandler<Upsert, GuildWelcomeMessageSettings>
        {
            public UpsertHandler(IDbContext db) : base(db) { }
            public override async Task<DbResult<GuildWelcomeMessageSettings>> Handle(Upsert request, CancellationToken cancellationToken)
            {
                GuildWelcomeMessageSettings settings = await this.DbContext.GuildWelcomeMessages
                    .FirstOrDefaultAsync(gw => gw.GuildId == request.GuildWelcomeMessage.GuildId, cancellationToken: cancellationToken);
                EntityEntry<GuildWelcomeMessageSettings> entity;
                DbResult<GuildWelcomeMessageSettings> result;
                if (settings is not null)
                {
                    settings.ShouldWelcomeMembers = request.GuildWelcomeMessage.ShouldWelcomeMembers;
                    settings.ShouldPing = request.GuildWelcomeMessage.ShouldPing;
                    entity = this.DbContext.GuildWelcomeMessages.Update(settings);
                    result = new DbResult<GuildWelcomeMessageSettings>
                    {
                        Success = entity.State.Equals(EntityState.Modified),
                        Value = entity.Entity,
                    };
                }
                else
                {
                    entity = this.DbContext.GuildWelcomeMessages.Add(request.GuildWelcomeMessage);
                    result = new DbResult<GuildWelcomeMessageSettings>
                    {
                        Success = entity.State.Equals(EntityState.Added),
                        Value = entity.Entity,
                    };
                }

                await this.DbContext.Context.SaveChangesAsync(cancellationToken);

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
            public DeleteHandler(IDbContext dbContext) : base(dbContext) { }

            public override async Task<DbResult> Handle(Delete request, CancellationToken cancellationToken)
            {
                GuildWelcomeMessageSettings guildWelcomeMessage = await this.DbContext.GuildWelcomeMessages.FirstOrDefaultAsync(gw => gw.GuildId == request.GuildId, cancellationToken: cancellationToken);
                if (guildWelcomeMessage == null)
                {
                    return new DbResult
                    {
                        Success = true,
                    };
                }

                EntityEntry<GuildWelcomeMessageSettings> entity = this.DbContext.GuildWelcomeMessages.Remove(guildWelcomeMessage);
                DbResult result = new DbResult
                {
                    Success = entity.State.Equals(EntityState.Deleted),
                };
                await this.DbContext.Context.SaveChangesAsync(cancellationToken);
                return result;
            }
        }

        public class GetGuildWelcomeMessageSettings : DbRequest<GuildWelcomeMessageSettings>
        {
            public GetGuildWelcomeMessageSettings(DiscordGuild guild) : this(guild.Id) { }

            public GetGuildWelcomeMessageSettings(ulong guildId)
            {
                this.GuildId = guildId;
            }

            public ulong GuildId { get; }
        }

        public class GetGuildWelcomeMessageSettingsHandler : DbRequestHandler<GetGuildWelcomeMessageSettings, GuildWelcomeMessageSettings>
        {
            public GetGuildWelcomeMessageSettingsHandler(IDbContext dbContext) : base(dbContext) { }

            public override async Task<DbResult<GuildWelcomeMessageSettings>> Handle(GetGuildWelcomeMessageSettings request, CancellationToken cancellationToken)
            {
                GuildWelcomeMessageSettings result = await this.DbContext.GuildWelcomeMessages
                    .FirstOrDefaultAsync(gw => gw.GuildId == request.GuildId, cancellationToken: cancellationToken);
                return new DbResult<GuildWelcomeMessageSettings>
                {
                    Success = result != null,
                    Value = result,
                };
            }
        }
    }
}
