using System.Diagnostics.CodeAnalysis;

namespace Norm.Utilities
{
    public class CustomResult<T>
    {
        [MemberNotNullWhen(true, nameof(Result))]
        public bool Success { get; private set; }
        public bool TimedOut { get; private set; }
        public bool Cancelled { get; private set; }
        public T? Result { get; private set; }
        public CustomResult(T? result = default, bool timedOut = false, bool cancelled = false)
        {
            this.Result = result;
            this.TimedOut = timedOut;
            this.Cancelled = cancelled;
            this.Success = this.TimedOut || this.Cancelled;
        }
    }
}
