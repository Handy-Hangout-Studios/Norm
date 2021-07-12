using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Norm.Database.Contexts;
using Norm.Database.Entities;
using Norm.Database.Requests.BaseClasses;
using Norm.Omdb.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Norm.Database.Requests
{
    public class GuildMovieNights
    {
        public class Add : DbRequest<GuildMovieNight>
        {
            public Add(string votingStartId, string votingEndId, string movieNightStartId, int numSuggestions, OmdbParentalRating maxRating, ulong guildId, ulong announcementChannelId, ulong hostId)
            {
                this.MovieNight = new GuildMovieNight(votingStartId, votingEndId, movieNightStartId, numSuggestions, maxRating, guildId, announcementChannelId, hostId);
            }

            public Add(string votingStartId, string votingEndId, string movieNightStartId, int numSuggestions, OmdbParentalRating maxRating, DiscordGuild guild, DiscordChannel announcementChannel, DiscordUser host) :
                this(votingStartId, votingEndId, movieNightStartId, numSuggestions, maxRating, guild.Id, announcementChannel.Id, host.Id)
            {

            }

            internal GuildMovieNight MovieNight { get; }
        }

        public class AddHandler : DbRequestHandler<Add, GuildMovieNight>
        {
            public AddHandler(NormDbContext dbContext) : base(dbContext)
            {

            }

            public override async Task<DbResult<GuildMovieNight>> Handle(Add request, CancellationToken cancellationToken)
            {
                EntityEntry<GuildMovieNight> entity = await this.DbContext.GuildMovieNights.AddAsync(request.MovieNight, cancellationToken);
                DbResult<GuildMovieNight> result = new()
                {
                    Success = entity.Entity != null && entity.State.Equals(EntityState.Added),
                    Value = entity.Entity,
                };

                await this.DbContext.SaveChangesAsync(cancellationToken);

                return result;
            }
        }

        public class Update : DbRequest<GuildMovieNight>
        {
            public Update(GuildMovieNight movieNight)
            {
                this.GuildMovieNight = movieNight;
            }

            internal GuildMovieNight GuildMovieNight { get; }
        }

        public class UpdateHandler : DbRequestHandler<Update, GuildMovieNight>
        {
            public UpdateHandler(NormDbContext context) : base(context) { }

            public override async Task<DbResult<GuildMovieNight>> Handle(Update request, CancellationToken cancellationToken)
            {
                EntityEntry<GuildMovieNight> entity = this.DbContext.GuildMovieNights.Update(request.GuildMovieNight);
                DbResult<GuildMovieNight> result = new()
                {
                    Success = entity.Entity != null && entity.State.Equals(EntityState.Modified),
                    Value = entity.Entity,
                };
                await this.DbContext.SaveChangesAsync(cancellationToken);
                return result;
            }
        }

        public class Delete : DbRequest
        {
            public Delete(GuildMovieNight movieNight)
            {
                this.GuildMovieNight = movieNight;
            }

            internal GuildMovieNight GuildMovieNight { get; }
        }

        public class DeleteHandler : DbRequestHandler<Delete>
        {
            public DeleteHandler(NormDbContext context) : base(context) { }

            public override async Task<DbResult> Handle(Delete request, CancellationToken cancellationToken)
            {
                EntityEntry<GuildMovieNight> entity = this.DbContext.GuildMovieNights.Remove(request.GuildMovieNight);
                DbResult result = new()
                {
                    Success = entity.State.Equals(EntityState.Deleted),
                };
                await this.DbContext.SaveChangesAsync(cancellationToken);
                return result;
            }
        }

        public class GetMovieNight : DbRequest<GuildMovieNight>
        {
            public GetMovieNight(int movieNightId)
            {
                this.MovieNightId = movieNightId;
            }

            internal int MovieNightId { get; }
        }

        public class GetMovieNightHandler : DbRequestHandler<GetMovieNight, GuildMovieNight>
        {
            public GetMovieNightHandler(NormDbContext context) : base(context)
            {

            }

            public override async Task<DbResult<GuildMovieNight>> Handle(GetMovieNight request, CancellationToken cancellationToken)
            {
                GuildMovieNight? gmn =
                    await this.DbContext.GuildMovieNights
                        .Include(gmn => gmn.MovieNightAndSuggestions)
                        .ThenInclude(row => row.MovieSuggestion)
                        .FirstOrDefaultAsync(gmn => gmn.Id == request.MovieNightId, cancellationToken: cancellationToken);

                return new DbResult<GuildMovieNight>
                {
                    Success = gmn != null,
                    Value = gmn,
                };
            }
        }

        public class GetAllGuildsMovieNights : DbRequest<IEnumerable<GuildMovieNight>>
        {
            public GetAllGuildsMovieNights(ulong guildId)
            {
                this.GuildId = guildId;
            }

            internal ulong GuildId { get; }
        }

        public class GetAllGuildsMovieNightsHandler : DbRequestHandler<GetAllGuildsMovieNights, IEnumerable<GuildMovieNight>>
        {
            public GetAllGuildsMovieNightsHandler(NormDbContext context) : base(context) { }

            public override async Task<DbResult<IEnumerable<GuildMovieNight>>> Handle(GetAllGuildsMovieNights request, CancellationToken cancellationToken)
            {
                try
                {
                    IEnumerable<GuildMovieNight> movieNights =
                        await this.DbContext.GuildMovieNights
                            .Include(gmn => gmn.MovieNightAndSuggestions)
                            .ThenInclude(row => row.MovieSuggestion)
                            .Where(gmn => gmn.GuildId == request.GuildId)
                            .ToListAsync(cancellationToken);

                    return new DbResult<IEnumerable<GuildMovieNight>>
                    {
                        Success = true,
                        Value = movieNights,
                    };
                }
                catch (Exception e) when (e is ArgumentNullException or OperationCanceledException)
                {
                    return new DbResult<IEnumerable<GuildMovieNight>>
                    {
                        Success = false,
                        Value = null,
                    };
                }
            }
        }
    }
}
