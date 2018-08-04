using Discord;
using Discord.Commands;
using System.Threading.Tasks;

namespace Umbreon.Interactive.Criteria
{
    public class EnsureSourceChannelCriterion : ICriterion<IMessage>
    {
        public Task<bool> JudgeAsync(ICommandContext sourceContext, IMessage parameter)
        {
            var ok = sourceContext.Channel.Id == parameter.Channel.Id;
            return Task.FromResult(ok);
        }
    }
}
