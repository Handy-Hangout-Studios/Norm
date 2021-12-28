using Microsoft.EntityFrameworkCore;
using Norm.DatabaseRewrite.Contexts;
using Norm.DatabaseRewrite.Requests.BaseClasses;

namespace Norm.DatabaseRewrite.Requests
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
