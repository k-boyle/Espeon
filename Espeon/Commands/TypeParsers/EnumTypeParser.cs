using Casino.Common.Qmmands;
using Qmmands;
using System;
using System.Threading.Tasks;
using Espeon.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Espeon.Commands
{
    public abstract class EnumTypeParser<T> : TypeParser<T> where T : Enum
    {
        private PrimiteTypeParser<T> _parser;

        public override ValueTask<TypeParserResult<T>> ParseAsync(Parameter parameter, string value, CommandContext ctx, IServiceProvider provider)
        {
            var commands = provider.GetService<CommandService>();

            _parser ??= commands.GetPrimiteTypeParser<T>();

            var result = _parser.TryParse(parameter, value, out var res);

            if (result)
                return TypeParserResult<T>.Successful(res);

            var packs = typeof(T).GetEnumNames();
            var toSend = string.Join(", ", packs);
            var response = provider.GetService<ResponseService>();
            var context = (EspeonContext)ctx;
            var user = context.Invoker;

            return TypeParserResult<T>.Unsuccessful(
                response.GetResponse(this, user.ResponsePack, 0, toSend));
        }
    }
}
