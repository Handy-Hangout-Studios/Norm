using Norm.Database.Contexts;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Norm.Database.Requests.BaseClasses
{
    public abstract class DbRequestHandler<T, U> : IRequestHandler<T, DbResult<U>> where T : IRequest<DbResult<U>>
    {
        public DbRequestHandler(IDbContext db)
        {
            this.DbContext = db;
        }
        public abstract Task<DbResult<U>> Handle(T request, CancellationToken cancellationToken);

        protected IDbContext DbContext { get; }
    }

    public abstract class DbRequestHandler<T> : IRequestHandler<T, DbResult> where T : IRequest<DbResult>
    {
        public DbRequestHandler(IDbContext dbContext)
        {
            this.DbContext = dbContext;
        }
        public abstract Task<DbResult> Handle(T request, CancellationToken cancellationToken);

        protected IDbContext DbContext { get; }
    }
}
