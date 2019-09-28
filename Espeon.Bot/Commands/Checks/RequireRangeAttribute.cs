using Espeon.Commands;
using Espeon.Services;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Espeon.Bot.Commands
{
    public class RequireRangeAttribute : EspeonParameterCheckBase
    {
        private readonly int _minValue;
        private readonly int _maxValue;

        public RequireRangeAttribute(int minValue) : this(minValue, int.MaxValue)
        {
        }

        public RequireRangeAttribute(int minValue, int maxValue)
        {
            if (maxValue <= minValue)
                throw new ArgumentOutOfRangeException($"{nameof(maxValue)} must be greater than {nameof(minValue)}");

            _minValue = minValue;
            _maxValue = maxValue;
        }

        public override ValueTask<CheckResult> CheckAsync(object argument, EspeonContext context, IServiceProvider provider)
        {
            var value = (int)argument;

            if (value >= _minValue && value < _maxValue)
                return CheckResult.Successful;

            var response = provider.GetService<IResponseService>();

            var user = context.Invoker;

            return CheckResult.Unsuccessful(response.GetResponse(this, user.ResponsePack, 0, _minValue, _maxValue));
        }
    }
}
