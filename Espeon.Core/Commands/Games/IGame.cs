using System.Threading.Tasks;

namespace Espeon.Core.Commands {
	public interface IGame : IReactionCallback {
		Task<bool> StartAsync();
		Task EndAsync();
	}
}