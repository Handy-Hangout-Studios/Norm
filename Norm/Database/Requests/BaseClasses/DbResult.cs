using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Norm.Database.Requests
{
    public class DbResult
    {
        public bool Success { get; init; }
    }

    public class DbResult<T> : DbResult
    {
        public T Value { get; init; }
    }
}
