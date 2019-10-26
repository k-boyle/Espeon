using Qmmands;
using System;
using System.Threading.Tasks;

namespace Espeon.Core.Commands {
	public abstract class EspeonTypeParser<T> : TypeParser<T> {
		public override ValueTask<TypeParserResult<T>> ParseAsync(Parameter parameter, string value,
			CommandContext context, IServiceProvider provider) {
			return ParseAsync(parameter, value, (EspeonContext) context, provider);
		}

		public abstract ValueTask<TypeParserResult<T>> ParseAsync(Parameter parameter, string value,
			EspeonContext context, IServiceProvider provider);
	}
}