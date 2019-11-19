using Disqord;
using Espeon.Core.Services;
using Kommon.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Services {
	public class EmotesService : BaseService<InitialiseArgs>, IEmoteService {
		private const string EmotesDir = "./Emotes/emotes.json";

		[Inject] private readonly DiscordClient _client;

		private readonly Dictionary<string, Lazy<CachedGuildEmoji>> _collection;
		public CachedGuildEmoji this[string key] => this._collection[key].Value;

		public EmotesService(IServiceProvider services) : base(services) {
			this._collection = new Dictionary<string, Lazy<CachedGuildEmoji>>();
		}

		public override Task InitialiseAsync(IServiceProvider services, InitialiseArgs args) {
			JObject emotesObject = JObject.Parse(File.ReadAllText(EmotesDir));

			foreach ((string key, JToken value) in emotesObject) {
				this._collection.Add(key,
					new Lazy<CachedGuildEmoji>(this._client.Guilds.SelectMany(x => x.Value.Emojis)
						.FirstOrDefault(y => y.ToString() == value.ToString()).Value));
			}

			return Task.CompletedTask;
		}
	}
}