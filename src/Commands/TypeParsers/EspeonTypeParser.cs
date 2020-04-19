using Qmmands;
using System.Threading.Tasks;

namespace Espeon {
    public abstract class EspeonTypeParser<T> : TypeParser<T> {
        public abstract ValueTask<TypeParserResult<T>> ParseAsync(
            Parameter parameter,
            string value,
            EspeonCommandContext context);

        public override ValueTask<TypeParserResult<T>> ParseAsync(
                Parameter parameter,
                string value,
                CommandContext context) {
            return ParseAsync(parameter, value, (EspeonCommandContext) context);
        }
    }
}