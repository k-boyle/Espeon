using Discord;
using Espeon.Core.Commands;
using System.Threading.Tasks;

namespace Espeon.Commands {
	public class UserCriteria : ICriterion<IUser>, ICriterion<IMessage> {
		private readonly ulong _userId;

		public UserCriteria(ulong userId) {
			this._userId = userId;
		}

		public Task<bool> JudgeAsync(EspeonContext context, IUser entity) {
			return Task.FromResult(entity.Id == this._userId);
		}

		public Task<bool> JudgeAsync(EspeonContext context, IMessage entity) {
			return Task.FromResult(this._userId == entity.Author.Id);
		}
	}
}