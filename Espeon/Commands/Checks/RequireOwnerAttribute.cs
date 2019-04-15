using Qmmands;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Espeon.Commands
{
    public class RequireOwnerAttribute : CheckAttribute
    {
        public override async ValueTask<CheckResult> CheckAsync(CommandContext originalContext, IServiceProvider provider)
        {
            var context = originalContext as EspeonContext;

            var app = await context.Client.GetApplicationInfoAsync();

            if (app.Owner.Id == context.User.Id)
                return CheckResult.Successful;

            var user = await context.GetInvokerAsync();

            var resp = new Dictionary<ResponsePack, string[]>
            {
                [ResponsePack.Default] = new[]
                {
                    $"{Module.Name} commands can only be used by the bot owner",
                    $"{Command.Name} can only be used by the bot owner"
                },

                [ResponsePack.owo] = new []
                {
                    $"only daddy can use {Module.Name}",
                    $"only daddy can use {Command.Name}"
                }
            };

            return CheckResult.Unsuccessful(Command is null ? resp[user.ResponsePack][0] : resp[user.ResponsePack][1]);
        }
    }
}
