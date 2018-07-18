using System.Collections.Generic;
using Microsoft.CodeAnalysis.Scripting;
using System.Linq;
using System.Reflection;
using Umbreon.Modules.Contexts;

namespace Umbreon.Helpers
{
    public static class EvalHelper
    {
        public static ScriptOptions AddNamespaces(this ScriptOptions options)
        {
            return options.WithImports(Assembly.GetExecutingAssembly().GetTypes().Select(x => x.Namespace).Distinct());
        }

        public static ScriptOptions AddEssemblies(this ScriptOptions options)
        {
            return options.WithReferences(GetAssemblies());
        }

        private static IEnumerable<Assembly> GetAssemblies()
        {
            var entries = Assembly.GetEntryAssembly();
            foreach (var assembly in entries.GetReferencedAssemblies())
                yield return Assembly.Load(assembly);
            yield return entries;
        }
    }

    public class Globals
    {
        public GuildCommandContext Context { get; set; }
    }
}
