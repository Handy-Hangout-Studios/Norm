using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Norm.Database.Contexts;
using Norm.Database.Entities;
using Norm.Database.Requests.BaseClasses;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Norm.Database.Requests
{
    public class NovelInfos
    {
        public class Add : DbRequest<NovelInfo>
        {
            public Add(ulong fictionId, string name, string syndicationUri, string fictionUri, ulong mostRecentChapterId)
            {
                this.Novel = new NovelInfo(fictionId, name, syndicationUri, fictionUri, mostRecentChapterId);
            }

            public NovelInfo Novel { get; }
        }

        public class AddHandler : DbRequestHandler<Add, NovelInfo>
        {
            public AddHandler(NormDbContext context) : base(context) { }

            public override async Task<DbResult<NovelInfo>> Handle(Add request, CancellationToken cancellationToken)
            {
                EntityEntry<NovelInfo> entity = this.DbContext.AllNovelInfo.Add(request.Novel);
                DbResult<NovelInfo> result = new()
                {
                    Success = entity.Entity != null && entity.State.Equals(EntityState.Added),
                    Value = entity.Entity,
                };
                await this.DbContext.SaveChangesAsync(cancellationToken);

                return result;
            }
        }

        public class Update : DbRequest<NovelInfo>
        {
            public Update(NovelInfo novel)
            {
                this.Novel = novel;
            }

            public NovelInfo Novel { get; }
        }

        public class UpdateHandler : DbRequestHandler<Update, NovelInfo>
        {
            public UpdateHandler(NormDbContext context) : base(context) { }

            public override async Task<DbResult<NovelInfo>> Handle(Update request, CancellationToken cancellationToken)
            {
                EntityEntry<NovelInfo> entity = this.DbContext.AllNovelInfo.Update(request.Novel);
                DbResult<NovelInfo> result = new()
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
            public Delete(NovelInfo novel)
            {
                this.Novel = novel;
            }

            public NovelInfo Novel { get; }
        }

        public class DeleteHandler : DbRequestHandler<Delete>
        {
            public DeleteHandler(NormDbContext dbContext) : base(dbContext) { }

            public override async Task<DbResult> Handle(Delete request, CancellationToken cancellationToken)
            {
                EntityEntry<NovelInfo> entity = this.DbContext.AllNovelInfo.Remove(request.Novel);
                DbResult result = new()
                {
                    Success = entity.State.Equals(EntityState.Deleted),
                };
                await this.DbContext.SaveChangesAsync(cancellationToken);
                return result;
            }
        }

        public class GetNovelInfo : DbRequest<NovelInfo>
        {
            public GetNovelInfo(int Id)
            {
                this.Id = Id;
            }

            public GetNovelInfo(ulong fictionId)
            {
                this.FictionId = fictionId;
            }

            public int Id { get; }
            public ulong FictionId { get; }
        }

        public class GetNovelInfoHandler : DbRequestHandler<GetNovelInfo, NovelInfo>
        {
            public GetNovelInfoHandler(NormDbContext context) : base(context) { }

            public override async Task<DbResult<NovelInfo>> Handle(GetNovelInfo request, CancellationToken cancellationToken)
            {
                NovelInfo? result = await this.DbContext.AllNovelInfo
                    .FirstOrDefaultAsync(n => n.FictionId == request.FictionId || n.Id == request.Id, cancellationToken: cancellationToken);

                return new DbResult<NovelInfo>
                {
                    Success = result != null,
                    Value = result,
                };
            }
        }

        public class GetAllNovelsInfo : DbRequest<IEnumerable<NovelInfo>>
        {
            public GetAllNovelsInfo() { }
        }

        public class GetAllNovelsInfoHandler : DbRequestHandler<GetAllNovelsInfo, IEnumerable<NovelInfo>>
        {
            public GetAllNovelsInfoHandler(NormDbContext context) : base(context) { }

            public override async Task<DbResult<IEnumerable<NovelInfo>>> Handle(GetAllNovelsInfo request, CancellationToken cancellationToken)
            {
                try
                {
                    IEnumerable<NovelInfo> allNovelsInfo = await this.DbContext.AllNovelInfo
                        .Include(n => n.AssociatedGuildNovelRegistrations)
                        .ToListAsync(cancellationToken: cancellationToken);
                    return new DbResult<IEnumerable<NovelInfo>>
                    {
                        Success = true,
                        Value = allNovelsInfo,
                    };
                }
                catch (Exception e) when (e is ArgumentNullException or OperationCanceledException)
                {
                    return new DbResult<IEnumerable<NovelInfo>>
                    {
                        Success = false,
                        Value = null,
                    };
                }
            }
        }
    }
}
