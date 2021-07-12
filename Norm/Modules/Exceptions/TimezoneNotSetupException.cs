using System;
using System.Runtime.Serialization;

namespace Norm.Modules.Exceptions
{
    [Serializable]
    internal class TimezoneNotSetupException : Exception
    {
        public TimezoneNotSetupException()
        {
        }

        public TimezoneNotSetupException(string? message) : base(message)
        {
        }

        public TimezoneNotSetupException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected TimezoneNotSetupException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}