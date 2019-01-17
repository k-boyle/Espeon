using Discord;
using Espeon.Commands;
using System.Threading.Tasks;

namespace Espeon.Interactive.Criteria
{
    public class ChannelCriteria : ICriterion<IChannel>, ICriterion<IMessage>
    {
        private readonly ulong _channelId;

        public ChannelCriteria(ulong channelId)
        {
            _channelId = channelId;
        }

        public Task<bool> JudgeAsync(EspeonContext context, IChannel entity)
        {
            return Task.FromResult(_channelId == entity.Id);
        }

        public Task<bool> JudgeAsync(EspeonContext context, IMessage entity)
        {
            return Task.FromResult(_channelId == entity.Channel.Id);
        }
    }
}
