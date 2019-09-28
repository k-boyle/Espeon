using Qmmands;
using System;
using System.Threading.Tasks;

namespace Espeon.Commands
{
    public abstract class EspeonParameterCheckBase : ParameterCheckAttribute
    {
        public override ValueTask<CheckResult> CheckAsync(object argument, CommandContext context,
            IServiceProvider provider)
            => CheckAsync(argument, (EspeonContext) context, provider);

        public abstract ValueTask<CheckResult> CheckAsync(object argument, EspeonContext context,
            IServiceProvider provider);
    }
}
