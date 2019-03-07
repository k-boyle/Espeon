using Qmmands;
using System;
using System.Threading.Tasks;

namespace Espeon.Commands
{
    public class RequireRangeAttribute : ParameterCheckBaseAttribute
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

        public override Task<CheckResult> CheckAsync(object argument, ICommandContext context, IServiceProvider provider)
        {
            var value = (int)argument;

            return Task.FromResult(
                value > _minValue && value <= _maxValue 
                ? CheckResult.Successful 
                : CheckResult.Unsuccessful($"Value must be between {_minValue} and {_maxValue}"));
        }
    }
}
