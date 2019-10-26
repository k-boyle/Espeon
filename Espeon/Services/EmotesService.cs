using Casino.DependencyInjection;
using Discord;
using Espeon.Core.Services;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Espeon.Services {
	public class EmotesService : BaseService<InitialiseArgs>, IEmoteService {
		private const string EmotesDir = "./Emotes/emotes.json";

		private readonly Dictionary<string, Emote> _collection;
		IDictionary<string, Emote> IEmoteService.Collection => this._collection;


		public EmotesService(IServiceProvider services) : base(services) {
			this._collection = new Dictionary<string, Emote>();
		}

		public override Task InitialiseAsync(IServiceProvider services, InitialiseArgs args) {
			JObject emotesObject = JObject.Parse(File.ReadAllText(EmotesDir));

			foreach ((string key, JToken value) in emotesObject) {
				this._collection.Add(key, Emote.Parse(value.ToString()));
			}

			return Task.CompletedTask;
		}
	}
}