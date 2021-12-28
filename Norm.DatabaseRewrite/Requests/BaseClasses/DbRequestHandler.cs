using MediatR;
using Norm.DatabaseRewrite.Contexts;

namespace Norm.DatabaseRewrite.Requests.BaseClasses
{
    public abstract class DbRequestHandler<TRequest, TResult> : 
        IRequestHandler<TRequest, DbResult<TResult>> where TRequest : IRequest<DbResult<TResult>>
    {
        public DbRequestHandler(NormDbContext db)
        {
            this.DbContext = db;
        }
        public abstract Task<DbResult<TResult>> Handle(TRequest request, CancellationToken cancellationToken);

        protected NormDbContext DbContext { get; }
    }

    public abstract class DbRequestHandler<T> : IRequestHandler<T, DbResult> where T : IRequest<DbResult>
    {
        public DbRequestHandler(NormDbContext dbContext)
        {
            this.DbContext = dbContext;
        }
        public abstract Task<DbResult> Handle(T request, CancellationToken cancellationToken);

        protected NormDbContext DbContext { get; }
    }
}
