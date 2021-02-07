using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Norm.Database.Requests
{
    public abstract class DbRequest : IRequest<DbResult>
    {
        protected DbRequest() { }
    }

    public abstract class DbRequest<T> : IRequest<DbResult<T>>
    {
        protected DbRequest() { }
    }
}
