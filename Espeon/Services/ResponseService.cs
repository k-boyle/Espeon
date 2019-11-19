using Espeon.Commands;
using Espeon.Core;
using Espeon.Core.Services;
using Kommon.DependencyInjection;
using Kommon.Qmmands;
using Newtonsoft.Json;
using Qmmands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Module = Qmmands.Module;

namespace Espeon.Services {
	public class ResponseService : BaseService<InitialiseArgs>, IResponseService {
		private const string CommandMapDir = "./Commands/commandsmap.json";
		private const string CheckParserMapDir = "./Commands/checkparsermap.json";

		private Dictionary<string, Dictionary<string, Dictionary<ResponsePack, string[]>>> _commandResponses;
		private Dictionary<string, Dictionary<ResponsePack, string[]>> _checksAndParsers;

		[Inject] private readonly CommandService _commands;

		public ResponseService(IServiceProvider services) : base(services) { }

		void IResponseService.LoadResponses(IEnumerable<Module> modules) {
			this._commandResponses = new Dictionary<string, Dictionary<string, Dictionary<ResponsePack, string[]>>>();

			var commandMap =
				JsonConvert
					.DeserializeObject<Dictionary<string, Dictionary<string, Dictionary<ResponsePack, string[]>>>>(
						File.ReadAllText(CommandMapDir));

			static string LeetIt(string input) {
				return input.Replace("o", "0", StringComparison.InvariantCultureIgnoreCase)
					.Replace("i", "1", StringComparison.InvariantCultureIgnoreCase)
					.Replace("z", "2", StringComparison.InvariantCultureIgnoreCase)
					.Replace("e", "3", StringComparison.InvariantCultureIgnoreCase)
					.Replace("a", "4", StringComparison.InvariantCultureIgnoreCase)
					.Replace("s", "5", StringComparison.InvariantCultureIgnoreCase)
					.Replace("g", "6", StringComparison.InvariantCultureIgnoreCase)
					.Replace("t", "7", StringComparison.InvariantCultureIgnoreCase);
			}

			foreach (Module module in modules) {
				if (commandMap.ContainsKey(module.Name)) {
					this._commandResponses.Add(module.Name,
						new Dictionary<string, Dictionary<ResponsePack, string[]>>());

					foreach (Command command in module.Commands) {
						if (commandMap[module.Name].ContainsKey(command.Name)) {
							Dictionary<ResponsePack, string[]> resp = commandMap[module.Name][command.Name];

							if (!resp.ContainsKey(ResponsePack.Leet) &&
							    resp.TryGetValue(ResponsePack.Default, out string[] def)) {
								resp[ResponsePack.Leet] = def.Select(LeetIt).ToArray();
							}

							this._commandResponses[module.Name][command.Name] = resp;
						} else {
							commandMap[module.Name].Add(command.Name, new Dictionary<ResponsePack, string[]>());
						}
					}
				} else {
					commandMap.Add(module.Name,
						module.Commands.ToDictionary(x => x.Name, x => new Dictionary<ResponsePack, string[]>()));
				}
			}

			File.WriteAllText(CommandMapDir, JsonConvert.SerializeObject(commandMap, Formatting.Indented));

			this._checksAndParsers = new Dictionary<string, Dictionary<ResponsePack, string[]>>();

			var checkParserMap =
				JsonConvert.DeserializeObject<Dictionary<string, Dictionary<ResponsePack, string[]>>>(
					File.ReadAllText(CheckParserMapDir));

			Assembly assembly = typeof(RequireOwnerAttribute).Assembly;
			Type[] types = assembly.GetTypes().ToArray();

			IEnumerable<Type> checkTypes = types.Where(x =>
				!x.IsAbstract && (x.IsSubclassOf(typeof(CheckAttribute)) ||
				                  x.IsSubclassOf(typeof(ParameterCheckAttribute))));

			IReadOnlyCollection<Type> parserTypes = this._commands.FindTypeParsers(assembly);

			foreach (Type type in checkTypes.Concat(parserTypes)) {
				if (checkParserMap.ContainsKey(type.Name)) {
					Dictionary<ResponsePack, string[]> resp = checkParserMap[type.Name];

					if (!resp.ContainsKey(ResponsePack.Leet) &&
					    resp.TryGetValue(ResponsePack.Default, out string[] def)) {
						resp[ResponsePack.Leet] = def.Select(LeetIt).ToArray();
					}

					this._checksAndParsers[type.Name] = resp;
				} else {
					checkParserMap[type.Name] = new Dictionary<ResponsePack, string[]>();
				}
			}

			File.WriteAllText(CheckParserMapDir, JsonConvert.SerializeObject(checkParserMap, Formatting.Indented));
		}

		string IResponseService.GetResponse(string module, string command, ResponsePack pack, int index,
			params object[] args) {
			return string.Format(this._commandResponses[module][command][pack][index], args);
		}

		string IResponseService.GetResponse(object obj, ResponsePack pack, int index, params object[] args) {
			return string.Format(this._checksAndParsers[obj.GetType().Name][pack][index], args);
		}
	}
}