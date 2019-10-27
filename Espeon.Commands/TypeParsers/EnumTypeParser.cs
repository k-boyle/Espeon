using Casino.Qmmands;
using Espeon.Core.Databases;
using Espeon.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Espeon.Commands {
	public abstract class EnumTypeParser<T> : EspeonTypeParser<T> where T : Enum {
		private PrimitiveTypeParser<T> _parser;
		private string[] _names;

		public override ValueTask<TypeParserResult<T>> ParseAsync(Parameter parameter, string value,
			EspeonContext context, IServiceProvider provider) {
			var commands = provider.GetService<CommandService>();

			this._parser??=commands.GetPrimiteTypeParser<T>();

			bool result = this._parser.TryParse(parameter, value, out T res);

			if (result) {
				return TypeParserResult<T>.Successful(res);
			}

			this._names??=typeof(T).GetEnumNames();

			string toSend = string.Join(", ", this._names);
			var response = provider.GetService<IResponseService>();
			User user = context.Invoker;

			return TypeParserResult<T>.Unsuccessful(response.GetResponse(this, user.ResponsePack, 0, toSend));
		}
	}
}