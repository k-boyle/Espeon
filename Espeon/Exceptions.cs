using System;

namespace Espeon
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

    public class QuahuLiedException : Exception
    {
        public QuahuLiedException(string message) : base($"QUAHU CANNOT BE TRUSTED: {message}")
        {
        }
    }

    public class ExpectedContextException : Exception
    {
        public ExpectedContextException(string message) : base($"Expected context type: {message}")
        {
        }
    }

    public class InvalidServiceException : Exception
    {
        public InvalidServiceException(string type) : base($"{type} is not a valid service")
        {
        }
    }
}
