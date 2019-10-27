using Qmmands;
using System;
using System.Threading.Tasks;

namespace Espeon.Commands {
	public abstract class EspeonTypeParser<T> : TypeParser<T> {
		public override ValueTask<TypeParserResult<T>> ParseAsync(Parameter parameter, string value,
			CommandContext context) {
			return ParseAsync(parameter, value, (EspeonContext) context, context.ServiceProvider);
		}

		public abstract ValueTask<TypeParserResult<T>> ParseAsync(Parameter parameter, string value,
			EspeonContext context, IServiceProvider provider);
	}
}