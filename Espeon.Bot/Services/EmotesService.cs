using Casino.DependencyInjection;
using Discord;
using Espeon.Services;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Espeon.Bot.Services
{
    public class EmotesService : BaseService<InitialiseArgs>, IEmoteService
    {
        private const string EmotesDir = "./Emotes/emotes.json";

        private readonly Dictionary<string, Emote> _collection;
        IDictionary<string, Emote> IEmoteService.Collection => _collection;


        public EmotesService(IServiceProvider services) : base(services)
        {
            _collection = new Dictionary<string, Emote>();
        }

        public override Task InitialiseAsync(IServiceProvider services, InitialiseArgs args)
        {
            var emotesObject = JObject.Parse(File.ReadAllText(EmotesDir));

            foreach(var (key, value) in emotesObject)
            {
                _collection.Add(key, Emote.Parse(value.ToString()));
            }

            return Task.CompletedTask;
        }
    }
}
