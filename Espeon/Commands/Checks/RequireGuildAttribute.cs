using Qmmands;
using System;
using System.Threading.Tasks;

namespace Espeon.Commands
{
    public class RequireGuildAttribute : CheckAttribute
    {
        private readonly ulong _id;

        public RequireGuildAttribute(ulong id)
        {
            _id = id;
        }

        public override ValueTask<CheckResult> CheckAsync(CommandContext originalContext, IServiceProvider provider)
        {
            var context = originalContext as EspeonContext;

            return new ValueTask<CheckResult>(context!.Guild.Id == _id
                ? CheckResult.Successful
                : CheckResult.Unsuccessful("Command cannot be run in this guild"));
        }
    }
}
