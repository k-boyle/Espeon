using System.Threading.Tasks;
using Disqord;
using Qmmands;

namespace Espeon
{
    public class NotOwnerAttribute : ParameterCheckAttribute {
        public override async ValueTask<CheckResult> CheckAsync(object argument, CommandContext _) {
            if (argument is not IMember member) {
                return CheckResult.Unsuccessful("Check can only be applied to a member");
            }
            
            var context = (EspeonCommandContext) _;
            var application = await context.Bot.GetCurrentApplicationAsync();
            return member.Id == application.Owner.Id
                ? CheckResult.Unsuccessful("Bot owner cannot be the target, tehe")
                : CheckResult.Successful;
        }
    }
}