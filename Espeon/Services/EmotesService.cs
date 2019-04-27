using Casino.Common.DependencyInjection;
using Discord;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Espeon.Services
{
    public class EmotesService : BaseService<InitialiseArgs>
    {
        private const string EmotesDir = "./Emotes/emotes.json";

        public Dictionary<string, Emote> Collection;

        public EmotesService(IServiceProvider services) : base(services)
        {
            Collection = new Dictionary<string, Emote>();
        }

        public override Task InitialiseAsync(IServiceProvider services, InitialiseArgs args)
        {
            var emotesObject = JObject.Parse(File.ReadAllText(EmotesDir));

            foreach(var (key, value) in emotesObject)
            {
                Collection.Add(key, Emote.Parse(value.ToString()));
            }

            return Task.CompletedTask;
        }
    }
}
