using System;
using System.Threading.Tasks;

namespace Espeon.Core.Services {
	public interface IGamesService<in TGame> {
		Task<bool> TryStartGameAsync(ulong userId, TGame game, TimeSpan timeout);
		Task<bool> TryLeaveGameAsync(ulong userId);
	}
}