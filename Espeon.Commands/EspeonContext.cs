using Disqord;
using Espeon.Core.Database;
using Espeon.Core.Database.CommandStore;
using Espeon.Core.Database.GuildStore;
using Espeon.Core.Database.UserStore;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Espeon.Commands {
	public class EspeonContext : CommandContext, IDisposable {
		public UserStore UserStore { get; }

		public GuildStore GuildStore { get; }

		private CommandStore _commandStore;
		public CommandStore CommandStore => this._commandStore ??= new CommandStore();

		public DiscordClient Client { get; }
		public CachedUserMessage Message { get; }
		public CachedMember Member { get; }
		public CachedGuild Guild => Member.Guild;
		public CachedTextChannel Channel { get; }
		public string PrefixUsed { get; }

		public User Invoker { get; private set; }
		public Guild CurrentGuild { get; private set; }

		private EspeonContext(IServiceProvider services, DiscordClient client, CachedUserMessage message,
			string prefix) : base(services) {
			Client = client;
			Message = message;
			Member = message.Author as CachedMember;
			Channel = message.Channel as CachedTextChannel;

			PrefixUsed = prefix ?? $"{Guild.CurrentMember.Mention} ";

			UserStore = new UserStore();
			GuildStore = new GuildStore();
		}

		public static async Task<EspeonContext> CreateAsync(IServiceProvider services, DiscordClient client,
			CachedUserMessage message, string prefix) {
			var ctx = new EspeonContext(services, client, message, prefix);
			ctx.Invoker = await ctx.UserStore.GetOrCreateUserAsync(ctx.Member);
			ctx.CurrentGuild = await ctx.GuildStore.GetOrCreateGuildAsync(ctx.Guild);

			return ctx;
		}

		private bool _disposedValue;

		private void Dispose(bool disposing) {
			if (!this._disposedValue) {
				if (disposing) {
					GuildStore.Dispose();
					UserStore.Dispose();
					this._commandStore?.Dispose();
				}

				this._disposedValue = true;
			}
		}

		public void Dispose() {
			Dispose(true);
		}
	}
}