using Espeon.Commands;
using Newtonsoft.Json;
using Qmmands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Module = Qmmands.Module;

namespace Espeon.Services
{
    public class ResponseService : BaseService
    {
        private const string CommandMapDir = "./Commands/commandsmap.json";
        private const string CheckParserMapDir = "./Commands/checkparsermap.json";

        private Dictionary<string, Dictionary<string, Dictionary<ResponsePack, string[]>>> _commandResponses;
        private Dictionary<string, Dictionary<ResponsePack, string[]>> _checksAndParsers;

        [Inject] private readonly CommandService _commands;

        public ResponseService(IServiceProvider services) : base(services)
        {
        }

        public void LoadResponses(IEnumerable<Module> modules)
        {
            _commandResponses = new Dictionary<string, Dictionary<string, Dictionary<ResponsePack, string[]>>>();

            var commandMap =
                JsonConvert
                    .DeserializeObject<Dictionary<string, Dictionary<string, Dictionary<ResponsePack, string[]>>>>(
                        File.ReadAllText(CommandMapDir));

            foreach (var module in modules)
            {
                if (commandMap.ContainsKey(module.Name))
                {
                    _commandResponses.Add(module.Name, new Dictionary<string, Dictionary<ResponsePack, string[]>>());

                    foreach (var command in module.Commands)
                    {
                        if (commandMap[module.Name].ContainsKey(command.Name))
                        {
                            _commandResponses[module.Name][command.Name] = commandMap[module.Name][command.Name];
                        }
                        else
                        {
                            commandMap[module.Name].Add(command.Name, new Dictionary<ResponsePack, string[]>());
                        }
                    }
                }
                else
                {
                    commandMap.Add(
                        module.Name,
                        module.Commands.ToDictionary(x => x.Name, x => new Dictionary<ResponsePack, string[]>()));
                }
            }

            File.WriteAllText(CommandMapDir, 
                JsonConvert.SerializeObject(commandMap, Formatting.Indented));

            _checksAndParsers = new Dictionary<string, Dictionary<ResponsePack, string[]>>();

            var checkParserMap = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<ResponsePack, string[]>>>(
                File.ReadAllText(CheckParserMapDir));

            var assembly = typeof(RequireOwnerAttribute).Assembly;
            var types = assembly.GetTypes().ToArray();

            var checkTypes = types.Where(x =>
                !x.IsAbstract &&
                x.IsSubclassOf(typeof(CheckAttribute)) || x.IsSubclassOf(typeof(ParameterCheckAttribute)));

            var parserTypes = Utilities.GetTypeParserTypes(_commands, assembly);

            var joint = checkTypes.Concat(parserTypes);

            foreach (var type in joint)
            {
                if (checkParserMap.ContainsKey(type.Name))
                {
                    _checksAndParsers[type.Name] = checkParserMap[type.Name];
                }
                else
                {
                    checkParserMap[type.Name] = new Dictionary<ResponsePack, string[]>();
                }
            }

            File.WriteAllText(CheckParserMapDir,
                JsonConvert.SerializeObject(checkParserMap, Formatting.Indented));
        }

        public string GetResponse(string module, string command, ResponsePack pack, int index, params object[] args)
            => string.Format(_commandResponses[module][command][pack][index], args);

        public string GetResponse(object obj, ResponsePack pack, int index, params object[] args)
            => string.Format(_checksAndParsers[obj.GetType().Name][pack][index], args);
    }
}
