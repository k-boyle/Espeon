using System;

namespace Espeon
{
    public class InvalidServiceException : Exception
    {
        public InvalidServiceException(string type) : base($"{type} is not a valid service")
        {
        }
    }
}
