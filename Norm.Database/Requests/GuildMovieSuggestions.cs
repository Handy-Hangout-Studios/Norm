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
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Norm.Database.Requests
{
    public class GuildMovieSuggestions
    {
        public class Add : DbRequest<GuildMovieSuggestion>
        {
            public Add(string imdbId, ulong suggestorId, string title, ulong guildId, OmdbParentalRating parentalRating)
            {
                this.MovieSuggestion = new GuildMovieSuggestion
                {
                    ImdbId = imdbId,
                    SuggesterId = suggestorId,
                    Title = title,
                    GuildId = guildId,
                    Rating = parentalRating,
                };
            }

            public Add(string imdbId, DiscordUser suggestor, string title, DiscordGuild guild, OmdbParentalRating parentalRating) :
                this(imdbId, suggestor.Id, title, guild.Id, parentalRating)
            {

            }

            internal GuildMovieSuggestion MovieSuggestion { get; }
        }

        public class AddHandler : DbRequestHandler<Add, GuildMovieSuggestion>
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

        public class UpdateHandler : DbRequestHandler<Update, GuildMovieSuggestion>
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

        public class DeleteHandler : DbRequestHandler<Delete>
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
            public GetMovieSuggestion(string movieSuggestionImdbId)
            {
                this.MovieSuggestionImdbId = movieSuggestionImdbId;
            }

            internal string MovieSuggestionImdbId { get; }
        }

        public class GetMovieSuggestionHandler : DbRequestHandler<GetMovieSuggestion, GuildMovieSuggestion>
        {
            public GetMovieSuggestionHandler(NormDbContext context) : base(context)
            {

            }

            public override async Task<DbResult<GuildMovieSuggestion>> Handle(GetMovieSuggestion request, CancellationToken cancellationToken)
            {
                GuildMovieSuggestion? gms = await this.DbContext.GuildMovieSuggestions.FirstOrDefaultAsync(gms => gms.ImdbId == request.MovieSuggestionImdbId, cancellationToken: cancellationToken);
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

        public class GetAllGuildsMovieNightsHandler : DbRequestHandler<GetRandomGuildMovieSuggestions, IEnumerable<GuildMovieSuggestion>>
        {
            public GetAllGuildsMovieNightsHandler(NormDbContext context) : base(context) { }

            public override async Task<DbResult<IEnumerable<GuildMovieSuggestion>>> Handle(GetRandomGuildMovieSuggestions request, CancellationToken cancellationToken)
            {
                try
                {
                    IEnumerable<GuildMovieSuggestion> movieNights = await this.DbContext
                        .GuildMovieSuggestions
                        .Where(gms => gms.GuildId == request.GuildId && gms.Rating <= request.MaximumRating)
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
    }
}
