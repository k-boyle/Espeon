using Discord;
using Espeon.Database;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Espeon.Services
{
    public class EmotesService : BaseService
    {
        private const string EmotesDir = "./Emotes.json";

        public Dictionary<string, Emote> Collection;

        public EmotesService()
        {
            Collection = new Dictionary<string, Emote>();
        }

        public override Task InitialiseAsync(DatabaseContext context, IServiceProvider services)
        {
            var emotesObject = JObject.Parse(File.ReadAllText(EmotesDir));

            foreach(var token in emotesObject)
            {
                Collection.Add(token.Key, Emote.Parse(token.Value.ToString()));
            }

            return Task.CompletedTask;
        }
    }
}
