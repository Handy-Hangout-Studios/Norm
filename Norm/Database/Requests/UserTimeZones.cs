using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Norm.Database.Contexts;
using Norm.Database.Entities;
using Norm.Database.Requests.BaseClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Norm.Database.Requests
{
    public class UserTimeZones
    {
        public class Add : DbRequest<UserTimeZone>
        {
            public Add(DiscordUser user, string timeZoneId) : this(user.Id, timeZoneId) { }

            public Add(ulong userId, string timeZoneId)
            {
                this.UserTimeZone = new UserTimeZoneDto
                {
                    UserId = userId,
                    TimeZoneId = timeZoneId,
                };
            }

            public UserTimeZoneDto UserTimeZone { get; }
        }

        public class AddHandler : DbRequestHandler<Add, UserTimeZone>
        {
            public AddHandler(IDbContext context) : base(context) { }

            public override async Task<DbResult<UserTimeZone>> Handle(Add request, CancellationToken cancellationToken)
            {
                EntityEntry<UserTimeZone> entity = await this.DbContext.UserTimeZones
                    .AddAsync(new UserTimeZone { UserId = (ulong) request.UserTimeZone.UserId, TimeZoneId = request.UserTimeZone.TimeZoneId }, cancellationToken);
                DbResult<UserTimeZone> result = new()
                {
                    Success = entity.State.Equals(EntityState.Added),
                    Value = entity.Entity,
                };
                await this.DbContext.Context.SaveChangesAsync(cancellationToken);
                return result;
            }
        }

        public class Update : DbRequest<UserTimeZone>
        {
            public Update(UserTimeZone timeZone)
            {
                this.UserTimeZone = timeZone;
            }

            public UserTimeZone UserTimeZone { get; }

        }

        public class UserTimeZoneDto
        {
            public int? Id { get; init; }
            public ulong? UserId { get; init; }
            public string TimeZoneId { get; init; }
        }

        public class UpdateHandler : DbRequestHandler<Update, UserTimeZone>
        {
            public UpdateHandler(IDbContext context) : base(context) { }

            public override async Task<DbResult<UserTimeZone>> Handle(Update request, CancellationToken cancellationToken)
            {
                EntityEntry<UserTimeZone> entity = this.DbContext.UserTimeZones.Update(request.UserTimeZone);
                DbResult<UserTimeZone> result = new()
                {
                    Success = entity.State.Equals(EntityState.Modified),
                    Value = entity.Entity,
                };
                await this.DbContext.Context.SaveChangesAsync(cancellationToken);
                return result;
            }
        }

        public class Delete : DbRequest
        {
            public Delete(UserTimeZone userTimeZone)
            {
                this.UserTimeZone = userTimeZone;
            }

            public UserTimeZone UserTimeZone { get; }
        }

        public class DeleteHandler : DbRequestHandler<Delete>
        {
            public DeleteHandler(IDbContext context) : base(context) { }

            public override async Task<DbResult> Handle(Delete request, CancellationToken cancellationToken)
            {
                EntityEntry<UserTimeZone> entity = this.DbContext.UserTimeZones.Remove(request.UserTimeZone);
                DbResult result = new DbResult
                {
                    Success = entity.State.Equals(EntityState.Deleted),
                };
                await this.DbContext.Context.SaveChangesAsync(cancellationToken);
                return result;
            }
        }

        public class GetUsersTimeZone : DbRequest<UserTimeZone>
        {
            public GetUsersTimeZone(DiscordUser user) : this(user.Id) { }

            public GetUsersTimeZone(ulong userId)
            {
                this.UserId = userId;
            }

            public ulong UserId { get; }
        }

        public class GetUsersTimeZoneHandler: DbRequestHandler<GetUsersTimeZone, UserTimeZone>
        {
            public GetUsersTimeZoneHandler(IDbContext context) : base(context) { }

            public override async Task<DbResult<UserTimeZone>> Handle(GetUsersTimeZone request, CancellationToken cancellationToken)
            {
                UserTimeZone result = await this.DbContext.UserTimeZones
                    .FirstOrDefaultAsync(tz => tz.UserId == request.UserId, cancellationToken: cancellationToken);
                return new DbResult<UserTimeZone>
                {
                    Success = true,
                    Value = result,
                };
            }
        }
    }
}
