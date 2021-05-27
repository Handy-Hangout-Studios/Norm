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
    public class GuildNovelRegistrations
    {
        public class Add : DbRequest<GuildNovelRegistration>
        {
            public Add(ulong guildId, ulong? announcementChannelId, bool pingEveryone, bool pingNoOne, ulong? roleId, int novelInfoId, ulong? memberId, bool isDm)
            {
                this.NovelRegistration = new GuildNovelRegistration(guildId, announcementChannelId, pingEveryone, pingNoOne, memberId, isDm, roleId, novelInfoId);
            }

            public GuildNovelRegistration NovelRegistration { get; }
        }

        public class AddHandler : DbRequestHandler<Add, GuildNovelRegistration>
        {
            public AddHandler(NormDbContext dbContext) : base(dbContext) { }

            public override async Task<DbResult<GuildNovelRegistration>> Handle(Add request, CancellationToken cancellationToken)
            {
                EntityEntry<GuildNovelRegistration> entity = this.DbContext.GuildNovelRegistrations.Add(request.NovelRegistration);
                DbResult<GuildNovelRegistration> result = new()
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
            public Delete(GuildNovelRegistration novelRegistration)
            {
                this.NovelRegistration = novelRegistration;
            }

            public GuildNovelRegistration NovelRegistration { get; }
        }

        public class DeleteHandler : DbRequestHandler<Delete>
        {
            public DeleteHandler(NormDbContext dbContext) : base(dbContext) { }

            public override async Task<DbResult> Handle(Delete request, CancellationToken cancellationToken)
            {
                EntityEntry<GuildNovelRegistration> entity = this.DbContext.GuildNovelRegistrations.Remove(request.NovelRegistration);
                DbResult result = new()
                {
                    Success = entity.Entity != null && entity.State.Equals(EntityState.Deleted),
                };
                await this.DbContext.SaveChangesAsync(cancellationToken);
                return result;
            }
        }

        public class GetGuildsNovelRegistrations : DbRequest<IEnumerable<GuildNovelRegistration>>
        {
            public GetGuildsNovelRegistrations(DiscordGuild guild) : this(guild.Id) { }
            public GetGuildsNovelRegistrations(ulong guildId)
            {
                this.GuildId = guildId;
            }

            public ulong GuildId { get; }
        }

        public class GetGuildsNovelRegistrationsHandler : DbRequestHandler<GetGuildsNovelRegistrations, IEnumerable<GuildNovelRegistration>>
        {
            public GetGuildsNovelRegistrationsHandler(NormDbContext dbContext) : base(dbContext) { }

            public override async Task<DbResult<IEnumerable<GuildNovelRegistration>>> Handle(GetGuildsNovelRegistrations request, CancellationToken cancellationToken)
            {
                try
                {
                    IEnumerable<GuildNovelRegistration> result = await this.DbContext.GuildNovelRegistrations
                    .Where(r => r.GuildId == request.GuildId)
                    .Include(r => r.NovelInfo)
                    .ToListAsync(cancellationToken: cancellationToken);

                    return new DbResult<IEnumerable<GuildNovelRegistration>>
                    {
                        Success = true,
                        Value = result,
                    };
                }
                catch (Exception e) when (e is ArgumentNullException or OperationCanceledException)
                {
                    return new DbResult<IEnumerable<GuildNovelRegistration>>
                    {
                        Success = false,
                        Value = null,
                    };
                }
            }
        }

        public class GetMemberNovelRegistrations : DbRequest<IEnumerable<GuildNovelRegistration>>
        {
            public GetMemberNovelRegistrations(DiscordMember member) : this(member.Id) { }
            public GetMemberNovelRegistrations(ulong memberId)
            {
                this.MemberId = memberId;
            }

            public ulong MemberId { get; }
        }

        public class GetMemberNovelRegistrationsHandler : DbRequestHandler<GetMemberNovelRegistrations, IEnumerable<GuildNovelRegistration>>
        {
            public GetMemberNovelRegistrationsHandler(NormDbContext dbContext) : base(dbContext) { }

            public override async Task<DbResult<IEnumerable<GuildNovelRegistration>>> Handle(GetMemberNovelRegistrations request, CancellationToken cancellationToken)
            {
                try
                {
                    IEnumerable<GuildNovelRegistration> result = await this.DbContext.GuildNovelRegistrations
                    .Where(r => r.MemberId == request.MemberId && r.MemberId != null)
                    .Include(r => r.NovelInfo)
                    .ToListAsync(cancellationToken: cancellationToken);

                    return new DbResult<IEnumerable<GuildNovelRegistration>>
                    {
                        Success = true,
                        Value = result,
                    };
                }
                catch
                {
                    return new DbResult<IEnumerable<GuildNovelRegistration>>
                    {
                        Success = false,
                        Value = null,
                    };
                }
            }
        }
    }
}
