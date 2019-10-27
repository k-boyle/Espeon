using Discord.WebSocket;
using Espeon.Core.Databases;
using Espeon.Core.Databases.CommandStore;
using Espeon.Core.Databases.GuildStore;
using Espeon.Core.Databases.UserStore;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Espeon.Commands {
	public class EspeonContext : CommandContext, IDisposable {
		public UserStore UserStore { get; }

		public GuildStore GuildStore { get; }

		private CommandStore _commandStore;
		public CommandStore CommandStore => this._commandStore ??= new CommandStore();

		public DiscordSocketClient Client { get; }
		public SocketUserMessage Message { get; }
		public SocketGuildUser User { get; }
		public SocketGuild Guild => User.Guild;
		public SocketTextChannel Channel { get; }
		public string PrefixUsed { get; }

		public User Invoker { get; private set; }
		public Guild CurrentGuild { get; private set; }

		private EspeonContext(IServiceProvider services, DiscordSocketClient client, SocketUserMessage message,
			string prefix) : base(services) {
			Client = client;
			Message = message;
			User = message.Author as SocketGuildUser;
			Channel = message.Channel as SocketTextChannel;

			PrefixUsed = prefix ?? $"{Guild.CurrentUser.Mention} ";

			UserStore = new UserStore();
			GuildStore = new GuildStore();
		}

		public static async Task<EspeonContext> CreateAsync(IServiceProvider services, DiscordSocketClient client,
			SocketUserMessage message, string prefix) {
			var ctx = new EspeonContext(services, client, message, prefix);
			ctx.Invoker = await ctx.UserStore.GetOrCreateUserAsync(ctx.User);
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