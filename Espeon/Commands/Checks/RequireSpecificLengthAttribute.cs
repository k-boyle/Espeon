using Qmmands;
using System;
using System.Threading.Tasks;

namespace Espeon.Commands
{
    public class RequireSpecificLengthAttribute : ParameterCheckBaseAttribute
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

        public override Task<CheckResult> CheckAsync(object argument, ICommandContext context, IServiceProvider provider)
        {
            var str = argument.ToString();

            return Task.FromResult(str.Length > _minLength && str.Length < _maxLength
                ? CheckResult.Successful
                : CheckResult.Unsuccessful($"String length must be between {_minLength} and {_maxLength}"));
        }
    }
}
