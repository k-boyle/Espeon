using Qmmands;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Espeon.Commands
{
    public class RequireSpecificLengthAttribute : ParameterCheckAttribute
    {
        private readonly int _minLength;
        private readonly int _maxLength;

        public RequireSpecificLengthAttribute(int maxLength) : this(0, maxLength)
        {
        }

        public RequireSpecificLengthAttribute(int minLength, int maxLength)
        {
            _minLength = minLength;
            _maxLength = maxLength;
        }

        public override ValueTask<CheckResult> CheckAsync(object argument, CommandContext ctx, IServiceProvider provider)
        {
            var str = argument.ToString();

            if (str.Length > _minLength && str.Length < _maxLength)
                return CheckResult.Successful;

            var resp = new Dictionary<ResponsePack, string>
            {
                [ResponsePack.Default] = $"String length must be between {_minLength} and {_maxLength}",
                [ResponsePack.owo] = $"ownnno urr mesage must be bweteen {_minLength} n {_maxLength}"
            };

            var context = (EspeonContext) ctx;

            var user = context.Invoker;

            return CheckResult.Unsuccessful(resp[user.ResponsePack]);
        }
    }
}
