using Espeon.Core;
using Espeon.Core.Services;
using Qmmands;
using System.Threading.Tasks;

namespace Espeon.Commands {
	//TODO renamed this stuff
	/*
	* Summaries
	* Checks?
	*/
	[Name("Management")]
	[RequireOwner]
	[Description("Commands modification")]
	public class Management : EspeonModuleBase {
		public ICommandManagementService Manager { get; set; }

		[Command("Alias")]
		[Name("Command Alias")]
		[Description("Add or removes an alias from the specified command")]
		public async Task CommandAliasAsync(Alias action, Command target, string value) {
			bool result;

			switch (action) {
				case Alias.Add:

					result = await Manager.AddAliasAsync(Context.CommandStore, target.Module, target.Name, value);

					if (result) {
						await SendOkAsync(0, value, target.Name);
						return;
					}

					await SendNotOkAsync(1, value, target.Name);

					break;
				case Alias.Remove:

					result = await Manager.RemoveAliasAsync(Context.CommandStore, target.Module, target.Name, value);

					if (result) {
						await SendOkAsync(2, value, target.Name);
						return;
					}

					await SendNotOkAsync(3, value, target.Name);

					break;
			}
		}

		[Command("Alias")]
		[Name("Module Alias")]
		[Description("Add or removes an alias from the specified module")]
		public async Task ModuleAliasAsync(Alias action, Module target, string value) {
			bool result;

			switch (action) {
				case Alias.Add:

					result = await Manager.AddAliasAsync(Context.CommandStore, target, value);

					if (result) {
						await SendOkAsync(0, value, target.Name);
						return;
					}

					await SendNotOkAsync(1, value, target.Name);

					break;
				case Alias.Remove:

					result = await Manager.RemoveAliasAsync(Context.CommandStore, target, value);

					if (result) {
						await SendOkAsync(2, value, target.Name);
						return;
					}

					await SendNotOkAsync(3, value, target.Name);

					break;
			}
		}
	}
}