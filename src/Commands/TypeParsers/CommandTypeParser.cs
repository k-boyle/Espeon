using Qmmands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Espeon.LocalisationStringKey;

namespace Espeon {
    public class CommandTypeParser : EspeonTypeParser<IEnumerable<Command>> {
        public override ValueTask<TypeParserResult<IEnumerable<Command>>> ParseAsync(
                Parameter parameter,
                string value,
                EspeonCommandContext context) {
            var commandService = (ICommandService) context.Bot;
            var commands = commandService.GetAllCommands();
            var matchingCommands = commands.Where(command => IsMatchingCommand(value, command)).ToList();

            return matchingCommands.Count > 0
                ? TypeParserResult<IEnumerable<Command>>.Successful(matchingCommands)
                : new EspeonTypeParserFailedResult<IEnumerable<Command>>(NO_MATCHING_COMMANDS);
        }

        private static bool IsMatchingCommand(string value, Command command) {
            return command.Aliases.Contains(value, StringComparer.InvariantCultureIgnoreCase)
                || command.Name.Contains(value, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}