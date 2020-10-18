using Qmmands;
using System;
using System.Linq;
using System.Threading.Tasks;
using static Espeon.LocalisationStringKey;

namespace Espeon {
    public class ModuleTypeParser : EspeonTypeParser<Module> {
        public override ValueTask<TypeParserResult<Module>> ParseAsync(
                Parameter parameter,
                string value,
                EspeonCommandContext context) {
            var commandService = (ICommandService) context.Bot;
            var modules = commandService.GetAllModules();
            var foundModule = modules.FirstOrDefault(module => IsMatchingModule(value, module));

            return foundModule is null
                ? new EspeonTypeParserFailedResult<Module>(MODULE_NOT_FOUND)
                : TypeParserResult<Module>.Successful(foundModule);
        }

        private static bool IsMatchingModule(string value, Module module) {
            return string.Equals(module.Name, value, StringComparison.InvariantCultureIgnoreCase)
                || module.FullAliases.Contains(value, StringComparer.InvariantCultureIgnoreCase);
        }
    }
}