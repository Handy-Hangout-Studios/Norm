using System;
using System.Runtime.Serialization;

namespace Norm.Modules.Exceptions
{
    [Serializable]
    internal class UserTimeoutException : Exception
    {
        public string Context => base.Message;

        private UserTimeoutException()
        {
        }

        public UserTimeoutException(string? context) : base(context)
        {
        }

        public UserTimeoutException(string? context, Exception? innerException) : base(context, innerException)
        {
        }

        protected UserTimeoutException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}