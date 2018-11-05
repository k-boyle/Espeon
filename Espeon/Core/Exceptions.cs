using System;

namespace Espeon.Core
{
    public class NotInterfaceException : Exception
    {
        public NotInterfaceException(string message) : base(message)
        {
        }
    }
}
