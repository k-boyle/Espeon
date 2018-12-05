using Espeon.Core.Attributes;
using Espeon.Core.Services;
using Qmmands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Espeon.Services
{
    [Service(typeof(IResponseService), true)]
    public class ResponseService : IResponseService
    {
        private const string MapDir = "./commands.json";

        private readonly IDictionary<string, Dictionary<string, Dictionary<string, string>>> _responseMap;

        public ResponseService()
        {            
            _responseMap = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();
        }

        public async Task<string> GetResponseAsync(Module module, Command command, string pack = "default", params string[] @params)
        {
            var response = _responseMap[module.Name][command.Name][pack];
            //TODO interpolation
            return response ?? "No response found";
        }

        public Task OnCommandsRegisteredAsync(IEnumerable<Module> modules)
        {
            var loadedMap = JsonConvert
                .DeserializeObject<Dictionary<string, Dictionary<string, Dictionary<string, string>>>>
                (File.ReadAllText(MapDir));

            foreach(var module in modules)
            {
                if(string.IsNullOrWhiteSpace(module.Name))
                    throw new ArgumentNullException(nameof(module.Name));

                if(!loadedMap.ContainsKey(module.Name))
                    loadedMap[module.Name] = new Dictionary<string, Dictionary<string, string>>();

                _responseMap.Add(module.Name, new Dictionary<string, Dictionary<string, string>>());

                var commands = module.Commands;

                foreach(var command in commands)
                {
                    if(string.IsNullOrWhiteSpace(command.Name))
                        throw new ArgumentNullException(nameof(command.Name));                    

                    if(!loadedMap[module.Name].ContainsKey(command.Name))
                        loadedMap[module.Name][command.Name] = new Dictionary<string, string>();

                    _responseMap[module.Name][command.Name] = loadedMap[module.Name][command.Name];
                }
            }

            File.WriteAllText(MapDir, JsonConvert.SerializeObject(loadedMap));

            return Task.CompletedTask;
        }
    }
}