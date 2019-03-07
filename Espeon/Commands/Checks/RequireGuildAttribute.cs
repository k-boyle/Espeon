using Qmmands;
using System;
using System.Threading.Tasks;

namespace Espeon.Commands
{
    public class RequireGuildAttribute : CheckBaseAttribute
    {
        private readonly ulong _id;

        public RequireGuildAttribute(ulong id)
        {
            _id = id;
        }

        public override Task<CheckResult> CheckAsync(ICommandContext originalContext, IServiceProvider provider)
        {
            var context = originalContext as EspeonContext;

            return Task.FromResult(context!.Guild.Id == _id
                ? CheckResult.Successful
                : CheckResult.Unsuccessful("Command cannot be run in this guild"));
        }
    }
}
