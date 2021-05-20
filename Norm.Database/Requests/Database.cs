using Microsoft.EntityFrameworkCore;
using Norm.Database.Contexts;
using Norm.Database.Requests.BaseClasses;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Norm.Database.Requests
{
    public class Database
    {
        public class Migrate : DbRequest { }
        public class MigrateHandler : DbRequestHandler<Migrate>
        {
            public MigrateHandler(NormDbContext db) : base(db) { }

            public override async Task<DbResult> Handle(Migrate request, CancellationToken cancellationToken)
            {
                try
                {
                    await this.DbContext.Database.MigrateAsync(cancellationToken);
                    return new DbResult
                    {
                        Success = true,
                    };
                }
                catch (OperationCanceledException)
                {
                    return new DbResult
                    {
                        Success = false,
                    };
                }
            }
        }
    }
}
