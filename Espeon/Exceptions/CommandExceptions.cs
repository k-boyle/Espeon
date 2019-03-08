using System;

namespace Espeon
{
    public class ExpectedContextException : Exception
    {
        public ExpectedContextException(string message) : base($"Expected context type: {message}")
        {
        }
    }
}
