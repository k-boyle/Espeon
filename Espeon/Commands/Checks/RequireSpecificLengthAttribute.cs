using Espeon.Services;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
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
            
            var context = (EspeonContext) ctx;
            var response = provider.GetService<ResponseService>();

            var user = context.Invoker;

            return CheckResult.Unsuccessful(
                response.GetResponse(this, user.ResponsePack, 0, _minLength, _maxLength));
        }
    }
}
