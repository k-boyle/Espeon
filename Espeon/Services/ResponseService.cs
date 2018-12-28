using Espeon.Commands;
using Espeon.Database;
using Espeon.Database.Entities;
using Newtonsoft.Json;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Services
{
    public class ResponseService : IService
    {
        private const string MapDir = "./commands.json";
        
        private readonly IDictionary<string, Dictionary<string, Dictionary<string, string>>> _responseMap;

        public ResponseService()
        {
            _responseMap = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();
        }

        public Task InitialiseAsync(DatabaseContext context, IServiceProvider services)
         => Task.CompletedTask;

        public Task<string> GetResponseAsync(Module module, Command command, string pack = "default", params object[] @params)
        {
            var response = _responseMap[module.Name][command.Name][pack];

            if (@params.Length > 0)
            {
                response = string.Format(response, @params);
            }

            return Task.FromResult(response ?? "No response found");
        }

        public Task OnCommandsRegisteredAsync(IEnumerable<Module> modules)
        {
            var loadedMap = JsonConvert
                .DeserializeObject<Dictionary<string, Dictionary<string, Dictionary<string, string>>>>
                (File.ReadAllText(MapDir)) ?? new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();

            foreach (var module in modules)
            {
                if (string.IsNullOrWhiteSpace(module.Name))
                    throw new ArgumentNullException(nameof(module.Name));

                if (!loadedMap.ContainsKey(module.Name))
                    loadedMap[module.Name] = new Dictionary<string, Dictionary<string, string>>();

                _responseMap.Add(module.Name, new Dictionary<string, Dictionary<string, string>>());

                var commands = module.Commands;

                foreach (var command in commands)
                {
                    if (string.IsNullOrWhiteSpace(command.Name))
                        throw new ArgumentNullException(nameof(command.Name));

                    if (!loadedMap[module.Name].ContainsKey(command.Name))
                        loadedMap[module.Name][command.Name] = new Dictionary<string, string>();

                    _responseMap[module.Name][command.Name] = loadedMap[module.Name][command.Name];
                }
            }

            File.WriteAllText(MapDir, JsonConvert.SerializeObject(loadedMap, Formatting.Indented));

            return Task.CompletedTask;
        }

        public async Task<string> GetUsersPackAsync(EspeonContext context, ulong id)
        {
            var user = await context.Database.Users.FindAsync(id);

            if (!(user is null)) return user.ResponsePack;

            user = new User
            {
                Id = id
            };

            await context.Database.Users.UpsertAsync(user);

            return user.ResponsePack;
        }

        public Task<ImmutableArray<string>> GetResponsesPacksAsync()
        {
            var commandMaps = _responseMap.Values;
            var responseMaps = commandMaps.SelectMany(x => x.Values);
            var responsePacks = responseMaps.SelectMany(x => x.Keys);

            return Task.FromResult(responsePacks.ToImmutableArray());
        }
    }
}