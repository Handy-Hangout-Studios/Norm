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
    public class GuildModerationAuditRecords
    {
        public class Add : DbRequest<GuildModerationAuditRecord>
        {
            public Add(ulong guildId, ulong modUserId, ulong userId, ModerationActionType action, string reason)
            {
                this.Record = new GuildModerationAuditRecord
                {
                    GuildId = guildId,
                    ModeratorUserId = modUserId,
                    UserId = userId,
                    ModerationAction = action,
                    Reason = reason
                };
            }

            internal GuildModerationAuditRecord Record { get; }
        }

        public class AddHandler : DbRequestHandler<Add, GuildModerationAuditRecord>
        {
            public AddHandler(NormDbContext db, IClock clock) : base(db)
            {
                this.Clock = clock;
            }

            public override async Task<DbResult<GuildModerationAuditRecord>> Handle(Add request, CancellationToken cancellationToken)
            {
                request.Record.Timestamp = this.Clock.GetCurrentInstant();
                EntityEntry<GuildModerationAuditRecord> entity = this.DbContext.GuildModerationAuditRecords.Add(request.Record);
                DbResult<GuildModerationAuditRecord> result = new()
                {
                    Success = entity.Entity != null && entity.State.Equals(EntityState.Added),
                    Value = entity.Entity,
                };

                await this.DbContext.SaveChangesAsync(cancellationToken);

                return result;
            }

            internal IClock Clock { get; private set; }
        }

        public class GetGuildModerationAuditRecords : DbRequest<IEnumerable<GuildModerationAuditRecord>>
        {
            public GetGuildModerationAuditRecords(DiscordGuild guild) : this(guild.Id) { }

            public GetGuildModerationAuditRecords(ulong guildId)
            {
                this.Filter = new AuditFilters
                {
                    GuildId = guildId,
                };
            }

            public GetGuildModerationAuditRecords WithModeratorId(DiscordUser user)
            {
                return this.WithModeratorId(user.Id);
            }

            public GetGuildModerationAuditRecords WithModeratorId(ulong userId)
            {
                this.Filter.ModeratorUserId = userId;
                return this;
            }

            public GetGuildModerationAuditRecords WithUserId(DiscordUser user)
            {
                return this.WithUserId(user.Id);
            }

            public GetGuildModerationAuditRecords WithUserId(ulong userId)
            {
                this.Filter.UserId = userId;
                return this;
            }

            public GetGuildModerationAuditRecords WithModerationActionType(ModerationActionType type)
            {
                this.Filter.ModerationAction = type;
                return this;
            }

            internal AuditFilters Filter { get; }
        }

        internal class AuditFilters
        {

            internal ulong GuildId { get; set; }

            internal ulong? ModeratorUserId { get; set; }

            internal ulong? UserId { get; set; }

            internal ModerationActionType ModerationAction { get; set; } = ModerationActionType.NONE;
        }

        public class GetGuildEventsHandler : DbRequestHandler<GetGuildModerationAuditRecords, IEnumerable<GuildModerationAuditRecord>>
        {
            public GetGuildEventsHandler(NormDbContext dbContext) : base(dbContext) { }

            public override async Task<DbResult<IEnumerable<GuildModerationAuditRecord>>> Handle(GetGuildModerationAuditRecords request, CancellationToken cancellationToken)
            {
                try
                {
                    AuditFilters filter = request.Filter;
                    List<GuildModerationAuditRecord> result = await this.DbContext.GuildModerationAuditRecords
                        .Where(record =>
                            record.GuildId == request.Filter.GuildId &&
                            (filter.ModeratorUserId == null || record.ModeratorUserId == filter.ModeratorUserId) &&
                            (filter.UserId == null || record.UserId == filter.UserId) &&
                            (filter.ModerationAction == ModerationActionType.NONE || record.ModerationAction == filter.ModerationAction))
                        .ToListAsync(cancellationToken: cancellationToken);

                    return new DbResult<IEnumerable<GuildModerationAuditRecord>>
                    {
                        Success = true,
                        Value = result,
                    };
                }
                catch (Exception e) when (e is ArgumentNullException or OperationCanceledException)
                {
                    return new DbResult<IEnumerable<GuildModerationAuditRecord>>
                    {
                        Success = false,
                        Value = null,
                    };
                }
            }
        }
    }
}
