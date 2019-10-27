using Espeon.Core.Databases.CommandStore;
using Qmmands;
using System.Threading.Tasks;

namespace Espeon.Core.Services {
	public interface ICommandManagementService {
		Task<bool> AddAliasAsync(CommandStore commandStore, Module module, string alias);
		Task<bool> AddAliasAsync(CommandStore commandStore, Module module, string command, string alias);
		Task<bool> RemoveAliasAsync(CommandStore commandStore, Module module, string alias);
		Task<bool> RemoveAliasAsync(CommandStore commandStore, Module module, string command, string alias);
	}
}