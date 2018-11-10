using System;

namespace Espeon.Core
{
    public class NotInterfaceException : Exception
    {
        public NotInterfaceException(string message) : base(message)
        {
        }
    }

    public class QuahuRenamedException : Exception
    {
        public QuahuRenamedException(string message) : base($"Quahu renamed {message} REEEEEEEEEEEEEEEEEEEEEEEEE")
        {
        }
    }
}
