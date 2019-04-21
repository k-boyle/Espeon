using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Collections;
using System.Reflection;
using System.Threading.Tasks;
using Espeon.Services;

namespace Espeon.Commands
{
    public class ResponsePackTypeParser : TypeParser<ResponsePack>
    {
        private MethodInfo _parseMethod;
        private object _parser;

        public override ValueTask<TypeParserResult<ResponsePack>> ParseAsync(Parameter parameter, string value,
            CommandContext ctx, IServiceProvider provider)
        {
            var commands = provider.GetService<CommandService>();

            if (_parseMethod is null || _parser is null)
            {
                var type = commands.GetType();
                var field = type.GetField("_primitiveTypeParsers",
                    BindingFlags.Instance | BindingFlags.NonPublic);

                if(field is null)
                    throw new QuahuRenamedException("_primitiveTypeParsers");

                var dict = (IDictionary)field.GetValue(commands);
                _parser = dict[typeof(ResponsePack)];

                type = _parser.GetType();

                _parseMethod = type.GetMethod("TryParse");

                if(_parseMethod is null)
                    throw new QuahuRenamedException("TypeParse");
            }

            var parameters = new object[] { parameter, value, null };

            var result = (bool)_parseMethod.Invoke(_parser, parameters);

            if(result)
                return TypeParserResult<ResponsePack>.Successful((ResponsePack)parameters[2]);

            var packs = typeof(ResponsePack).GetEnumNames();
            var toSend = string.Join(", ", packs);
            var response = provider.GetService<ResponseService>();
            var context = (EspeonContext)ctx;
            var user = context.Invoker;

            return TypeParserResult<ResponsePack>.Unsuccessful(
                response.GetResponse(this, user.ResponsePack, 0, toSend));
        }
    }
}
