using Qmmands;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Espeon
{
    public static class Utilities
    {
        public static bool AvailableName(IEnumerable<Command> commands, string name)
            => commands.Any(x => x.FullAliases
                .Any(y => string.Equals(y, name, StringComparison.InvariantCultureIgnoreCase)));
    }
}
