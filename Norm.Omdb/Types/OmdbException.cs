using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Norm.Omdb.Types
{
    public class OmdbException : Exception
    {
        public OmdbException(string? message) : base(message)
        {
        }
    }
}
