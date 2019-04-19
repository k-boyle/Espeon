using Qmmands;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Espeon.Commands
{
    public class RequireRangeAttribute : ParameterCheckAttribute
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

        public override ValueTask<CheckResult> CheckAsync(object argument, CommandContext ctx, IServiceProvider provider)
        {
            var value = (int)argument;

            var resp = new Dictionary<ResponsePack, string>
            {
                [ResponsePack.Default] = $"Value must be between {_minValue} and {_maxValue}",
                [ResponsePack.owo] = $"oowwww walue mst be bitween {_minValue} and {_maxValue}"
            };

            if (value >= _minValue && value < _maxValue)
                return CheckResult.Successful;

            var context = (EspeonContext) ctx;

            var user = context.Invoker;

            return CheckResult.Unsuccessful(resp[user.ResponsePack]);
        }
    }
}
