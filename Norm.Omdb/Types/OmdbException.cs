using System;

namespace Norm.Omdb.Types
{
    public class OmdbException : Exception
    {
        public OmdbException(string? message) : base(message)
        {
        }
    }
}
