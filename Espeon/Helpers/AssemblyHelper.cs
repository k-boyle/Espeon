using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Espeon.Helpers
{
    public static class AssemblyHelper
    {
        public static IEnumerable<Type> GetAllTypesWithAttribute<T>() where T : Attribute
            => Assembly.GetEntryAssembly().GetTypes().Where(y => y.GetCustomAttributes(typeof(T), true).Length > 0);
    }
}
