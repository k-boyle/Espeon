using Espeon.Commands;
using Espeon.Services;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Espeon.Bot.Commands
{
    public class RequireSpecificLengthAttribute : EspeonParameterCheckBase
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

        public override ValueTask<CheckResult> CheckAsync(object argument, EspeonContext context, IServiceProvider provider)
        {
            var str = argument.ToString();

            if (str.Length > _minLength && str.Length < _maxLength)
                return CheckResult.Successful;

            var response = provider.GetService<IResponseService>();

            var user = context.Invoker;

            return CheckResult.Unsuccessful(
                response.GetResponse(this, user.ResponsePack, 0, _minLength, _maxLength));
        }
    }
}
