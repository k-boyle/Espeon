using System;
using System.Collections.Generic;
using System.Linq;

namespace Umbreon.Helpers
{
    public static class AssemblyHelper
    {
        public static IEnumerable<Type> GetAllTypesWithAttribute<T>() where T : Attribute
            => AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
                .Where(y => y.GetCustomAttributes(typeof(T), true).Length > 0);
    }
}
