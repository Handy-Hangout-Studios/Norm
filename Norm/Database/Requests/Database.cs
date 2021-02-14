using Microsoft.EntityFrameworkCore;
using Norm.Database.Contexts;
using Norm.Database.Requests.BaseClasses;
using System.Threading;
using System.Threading.Tasks;

namespace Norm.Database.Requests
{
    public class Database
    {
        public class Migrate : DbRequest { }
        public class MigrateHandler : DbRequestHandler<Migrate>
        {
            public MigrateHandler(IDbContext db) : base(db) { }

            public override async Task<DbResult> Handle(Migrate request, CancellationToken cancellationToken)
            {
                await this.DbContext.Context.Database.MigrateAsync(cancellationToken);
                return new DbResult
                {
                    Success = true,
                };
            }
        }
    }
}
