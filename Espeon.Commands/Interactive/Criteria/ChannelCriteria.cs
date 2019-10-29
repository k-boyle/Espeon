using Disqord;
using System.Threading.Tasks;

namespace Espeon.Commands {
	public class ChannelCriteria : ICriterion<IChannel>, ICriterion<IMessage> {
		private readonly ulong _channelId;

		public ChannelCriteria(ulong channelId) {
			this._channelId = channelId;
		}

		public ValueTask<bool> JudgeAsync(EspeonContext context, IChannel entity) {
			return new ValueTask<bool>(this._channelId == entity.Id);
		}

		public ValueTask<bool> JudgeAsync(EspeonContext context, IMessage entity) {
			return new ValueTask<bool>(this._channelId == entity.ChannelId);
		}
	}
}