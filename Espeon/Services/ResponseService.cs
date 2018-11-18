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
        [Inject] private readonly CommandService _commands;

        private const string MapDir = "./commands.json";

        private readonly IDictionary<string, Dictionary<string, string[]>> _responseMap;

        public ResponseService()
        {
            if (File.Exists(MapDir))
            {
                _responseMap =
                    JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string[]>>>(
                        File.ReadAllText(MapDir));
            }
            else
            {
                _responseMap = new Dictionary<string, Dictionary<string, string[]>>();
            }

            _commands.ModuleBuilding += OnModuleBuildingAsync;
        }

        private async Task OnModuleBuildingAsync(ModuleBuilder moduleBuilder)
        {
            if (_responseMap.ContainsKey(moduleBuilder.Name))
            {

            }
            else
            {
                
            }
        }

        public async Task<string> GetResponseAsync(Module module, Command command)
        {
            throw new NotImplementedException();
        }
    }
}