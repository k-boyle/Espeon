using System;

namespace Espeon.Exceptions
{
    public class ExpectedContextException : Exception
    {
        public ExpectedContextException(string message) : base($"Expected context type: {message}")
        {
        }
    }
}
