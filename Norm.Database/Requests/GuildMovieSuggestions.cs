using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using NodaTime;
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
    public class GuildMovieSuggestions
    {
        public class Add : DbRequest<GuildMovieSuggestion>
        {
            public Add(string imdbId, ulong suggestorId, string title, ulong guildId, int year, OmdbParentalRating parentalRating)
            {
                this.MovieSuggestion = new GuildMovieSuggestion(imdbId, suggestorId, title, guildId, year, parentalRating);
            }

            public Add(string imdbId, DiscordUser suggestor, string title, DiscordGuild guild, int year, OmdbParentalRating parentalRating) :
                this(imdbId, suggestor.Id, title, guild.Id, year, parentalRating)
            {

            }

            internal GuildMovieSuggestion MovieSuggestion { get; }
        }

        internal class AddHandler : DbRequestHandler<Add, GuildMovieSuggestion>
        {
            public AddHandler(NormDbContext dbContext) : base(dbContext)
            {

            }

            public override async Task<DbResult<GuildMovieSuggestion>> Handle(Add request, CancellationToken cancellationToken)
            {
                EntityEntry<GuildMovieSuggestion> entity = await this.DbContext.GuildMovieSuggestions.AddAsync(request.MovieSuggestion, cancellationToken);
                DbResult<GuildMovieSuggestion> result = new()
                {
                    Success = entity.Entity != null && entity.State.Equals(EntityState.Added),
                    Value = entity.Entity,
                };

                await this.DbContext.SaveChangesAsync(cancellationToken);

                return result;
            }
        }

        public class Update : DbRequest<GuildMovieSuggestion>
        {
            public Update(GuildMovieSuggestion movieSuggestion)
            {
                this.GuildMovieSuggestion = movieSuggestion;
            }

            internal GuildMovieSuggestion GuildMovieSuggestion { get; }
        }

        internal class UpdateHandler : DbRequestHandler<Update, GuildMovieSuggestion>
        {
            public UpdateHandler(NormDbContext context) : base(context) { }

            public override async Task<DbResult<GuildMovieSuggestion>> Handle(Update request, CancellationToken cancellationToken)
            {
                EntityEntry<GuildMovieSuggestion> entity = this.DbContext.GuildMovieSuggestions.Update(request.GuildMovieSuggestion);
                DbResult<GuildMovieSuggestion> result = new()
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
            public Delete(GuildMovieSuggestion movieSuggestion)
            {
                this.GuildMovieSuggestion = movieSuggestion;
            }

            internal GuildMovieSuggestion GuildMovieSuggestion { get; }
        }

        internal class DeleteHandler : DbRequestHandler<Delete>
        {
            public DeleteHandler(NormDbContext context) : base(context) { }

            public override async Task<DbResult> Handle(Delete request, CancellationToken cancellationToken)
            {
                EntityEntry<GuildMovieSuggestion> entity = this.DbContext.GuildMovieSuggestions.Remove(request.GuildMovieSuggestion);
                DbResult result = new()
                {
                    Success = entity.State.Equals(EntityState.Deleted),
                };
                await this.DbContext.SaveChangesAsync(cancellationToken);
                return result;
            }
        }

        public class GetMovieSuggestion : DbRequest<GuildMovieSuggestion>
        {
            public GetMovieSuggestion(string movieSuggestionImdbId, ulong guildId)
            {
                this.MovieSuggestionImdbId = movieSuggestionImdbId;
                this.GuildId = guildId;
            }

            public GetMovieSuggestion(string movieSuggestionImdbId, DiscordGuild guild) : this(movieSuggestionImdbId, guild.Id)
            {

            }

            internal string MovieSuggestionImdbId { get; }
            internal ulong GuildId { get; }
        }

        internal class GetMovieSuggestionHandler : DbRequestHandler<GetMovieSuggestion, GuildMovieSuggestion>
        {
            public GetMovieSuggestionHandler(NormDbContext context) : base(context)
            {

            }

            public override async Task<DbResult<GuildMovieSuggestion>> Handle(GetMovieSuggestion request, CancellationToken cancellationToken)
            {
                GuildMovieSuggestion? gms =
                    await this.DbContext.GuildMovieSuggestions
                        .Include(gms => gms.MovieNightAndSuggestions)
                        .ThenInclude(row => row.MovieNight)
                        .FirstOrDefaultAsync(gms => gms.ImdbId == request.MovieSuggestionImdbId && gms.GuildId == request.GuildId, cancellationToken: cancellationToken);

                return new DbResult<GuildMovieSuggestion>
                {
                    Success = gms != null,
                    Value = gms,
                };
            }
        }

        public class GetRandomGuildMovieSuggestions : DbRequest<IEnumerable<GuildMovieSuggestion>>
        {
            public GetRandomGuildMovieSuggestions(ulong guildId, int numSuggestions, OmdbParentalRating maximumRating)
            {
                this.GuildId = guildId;
                this.NumberOfSuggestions = numSuggestions;
                this.MaximumRating = maximumRating;
            }

            public GetRandomGuildMovieSuggestions(DiscordGuild guild, int numSuggestions, OmdbParentalRating maximumRating)
                : this(guild.Id, numSuggestions, maximumRating)
            {

            }

            internal ulong GuildId { get; }
            internal int NumberOfSuggestions { get; }
            internal OmdbParentalRating MaximumRating { get; }
        }

        internal class GetRandomGuildMovieSuggestionsHandler : DbRequestHandler<GetRandomGuildMovieSuggestions, IEnumerable<GuildMovieSuggestion>>
        {
            private readonly IClock clock;
            public GetRandomGuildMovieSuggestionsHandler(NormDbContext context, IClock clock) : base(context)
            {
                this.clock = clock;
            }

            public override async Task<DbResult<IEnumerable<GuildMovieSuggestion>>> Handle(GetRandomGuildMovieSuggestions request, CancellationToken cancellationToken)
            {
                try
                {
                    Instant oneYearAgo = this.clock.GetCurrentInstant() - Duration.FromDays(365);
                    IEnumerable<GuildMovieSuggestion> movieNights = await this.DbContext
                        .GuildMovieSuggestions
                        .Include(gms => gms.MovieNightAndSuggestions)
                        .ThenInclude(row => row.MovieNight)
                        .Where(gms => gms.GuildId == request.GuildId &&
                            (gms.Rating <= request.MaximumRating || gms.Year <= 1968) &&
                            (gms.InstantWatched == null || gms.InstantWatched > oneYearAgo)
                        )
                        .OrderBy(gms => EF.Functions.Random())
                        .Take(request.NumberOfSuggestions)
                        .ToListAsync(cancellationToken: cancellationToken);

                    return new DbResult<IEnumerable<GuildMovieSuggestion>>
                    {
                        Success = true,
                        Value = movieNights,
                    };
                }
                catch (Exception e) when (e is ArgumentNullException or OperationCanceledException)
                {
                    return new DbResult<IEnumerable<GuildMovieSuggestion>>
                    {
                        Success = false,
                        Value = null,
                    };
                }
            }
        }

        public class GetGuildMovieSuggestions : DbRequest<IEnumerable<GuildMovieSuggestion>>
        {
            public GetGuildMovieSuggestions(ulong guildId)
            {
                this.GuildId = guildId;
            }

            public GetGuildMovieSuggestions(DiscordGuild guild)
                : this(guild.Id)
            {

            }

            internal ulong GuildId { get; }
        }

        internal class GetGuildMovieSuggestionsHandler : DbRequestHandler<GetGuildMovieSuggestions, IEnumerable<GuildMovieSuggestion>>
        {
            public GetGuildMovieSuggestionsHandler(NormDbContext context) : base(context)
            {
            }

            public override async Task<DbResult<IEnumerable<GuildMovieSuggestion>>> Handle(GetGuildMovieSuggestions request, CancellationToken cancellationToken)
            {
                try
                {
                    IEnumerable<GuildMovieSuggestion> movieNights = await this.DbContext
                        .GuildMovieSuggestions
                        .Where(gms => gms.GuildId == request.GuildId)
                        .ToListAsync(cancellationToken: cancellationToken);

                    return new DbResult<IEnumerable<GuildMovieSuggestion>>
                    {
                        Success = true,
                        Value = movieNights,
                    };
                }
                catch (Exception e) when (e is ArgumentNullException or OperationCanceledException)
                {
                    return new DbResult<IEnumerable<GuildMovieSuggestion>>
                    {
                        Success = false,
                        Value = null,
                    };
                }
            }
        }

        public class GetUsersGuildMovieSuggestions : DbRequest<IEnumerable<GuildMovieSuggestion>>
        {
            public GetUsersGuildMovieSuggestions(ulong guildId, ulong userId)
            {
                this.GuildId = guildId;
            }

            public GetUsersGuildMovieSuggestions(DiscordGuild guild, DiscordUser user)
                : this(guild.Id, user.Id)
            {

            }

            internal ulong GuildId { get; }
            internal ulong UserId { get; }
        }

        internal class GetUsersGuildMovieSuggestionsHandler : DbRequestHandler<GetUsersGuildMovieSuggestions, IEnumerable<GuildMovieSuggestion>>
        {
            public GetUsersGuildMovieSuggestionsHandler(NormDbContext context) : base(context)
            {
            }

            public override async Task<DbResult<IEnumerable<GuildMovieSuggestion>>> Handle(GetUsersGuildMovieSuggestions request, CancellationToken cancellationToken)
            {
                try
                {
                    IEnumerable<GuildMovieSuggestion> movieNights = await this.DbContext
                        .GuildMovieSuggestions
                        .Where(gms => gms.GuildId == request.GuildId && gms.SuggesterId == request.UserId)
                        .ToListAsync(cancellationToken: cancellationToken);
                    
                    return new DbResult<IEnumerable<GuildMovieSuggestion>>
                    {
                        Success = true,
                        Value = movieNights,
                    };
                }
                catch (Exception e) when (e is ArgumentNullException or OperationCanceledException)
                {
                    return new DbResult<IEnumerable<GuildMovieSuggestion>>
                    {
                        Success = false,
                        Value = null,
                    };
                }
            }
        }
    }
}
