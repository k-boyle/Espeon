using Disqord;
using System.Threading.Tasks;

namespace Espeon.Commands {
	public class UserCriteria : ICriterion<IUser>, ICriterion<IMessage> {
		private readonly ulong _userId;

		public UserCriteria(ulong userId) {
			this._userId = userId;
		}

		public ValueTask<bool> JudgeAsync(EspeonContext context, IUser entity) {
			return new ValueTask<bool>(entity.Id == this._userId);
		}

		public ValueTask<bool> JudgeAsync(EspeonContext context, IMessage entity) {
			return new ValueTask<bool>(this._userId == entity.Author.Id);
		}
	}
}