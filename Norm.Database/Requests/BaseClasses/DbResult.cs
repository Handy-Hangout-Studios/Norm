using System.Diagnostics.CodeAnalysis;

namespace Norm.Database.Requests
{
    public class DbResult
    {
        public bool Success { get; internal init; }
    }

    public class DbResult<T> : DbResult
    {
        internal T? Value { private get; init; }

        public bool TryGetValue([NotNullWhen(true)] out T? value)
        {
            value = this.Value;
            return this.Success;
        }
    }
}
