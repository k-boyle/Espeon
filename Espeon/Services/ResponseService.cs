using Newtonsoft.Json;
using Qmmands;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Espeon.Services
{
    public class ResponseService : BaseService
    {
        private const string Dir = "./Commands/commandsmap.json";

        private Dictionary<string, Dictionary<string, Dictionary<ResponsePack, string[]>>> _responses;

        public void LoadResponses(IEnumerable<Module> modules)
        {
            _responses = new Dictionary<string, Dictionary<string, Dictionary<ResponsePack, string[]>>>();

            var commandMap =
                JsonConvert
                    .DeserializeObject<Dictionary<string, Dictionary<string, Dictionary<ResponsePack, string[]>>>>
                        (File.ReadAllText(Dir));

            foreach(var module in modules)
            {
                if(commandMap.ContainsKey(module.Name))
                {
                    _responses.Add(module.Name, new Dictionary<string, Dictionary<ResponsePack, string[]>>());

                    foreach(var command in module.Commands)
                    {
                        if(commandMap[module.Name].ContainsKey(command.Name))
                        {
                            _responses[module.Name][command.Name] = commandMap[module.Name][command.Name];
                        }
                        else
                        {
                            commandMap.Add(command.Name, new Dictionary<string, Dictionary<ResponsePack, string[]>>());
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

            File.WriteAllText(Dir, JsonConvert.SerializeObject(commandMap, Formatting.Indented));
        }

        public Dictionary<ResponsePack, string[]> GetResponses(string module, string command)
            => _responses[module][command];
    }
}
