using System;

namespace Espeon
{
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

    public class ThisWasQuahusFaultException : Exception
    {
        public ThisWasQuahusFaultException() : base("Quahu told you FAKE NEWS. SHE CAN'T BE TRUSTED!!")
        {
        }
    }
}
