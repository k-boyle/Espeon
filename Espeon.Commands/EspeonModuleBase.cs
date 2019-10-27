using Discord;
using Discord.WebSocket;
using Espeon.Core.Databases;
using Espeon.Core.Services;
using Qmmands;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Espeon.Commands {
	public abstract class EspeonModuleBase : ModuleBase<EspeonContext>, IDisposable {
		public IMessageService Message { get; set; }
		public IInteractiveService<IReactionCallback, EspeonContext> Interactive { get; set; }
		public IResponseService Responses { get; set; }
		public IServiceProvider Services { get; set; }

		protected SocketTextChannel Channel => Context.Channel;
		protected SocketGuildUser User => Context.User;
		protected SocketGuild Guild => Context.Guild;
		protected DiscordSocketClient Client => Context.Client;

		protected Task<IUserMessage> SendMessageAsync(Embed embed) {
			return SendMessageAsync(string.Empty, embed);
		}

		protected async Task<IUserMessage> SendMessageAsync(string content, Embed embed = null) {
			return await Message.SendAsync(Context.Message, x => {
				x.Content = content;
				x.Embed = embed;
			});
		}

		protected async Task<IUserMessage> SendFileAsync(Stream stream, string fileName, string content = null,
			Embed embed = null) {
			return await Message.SendAsync(Context.Message, x => {
				x.Content = content;
				x.Embed = embed;
				x.Stream = stream;
				x.FileName = fileName;
			});
		}

		protected async Task<IUserMessage> SendOkAsync(int index, params object[] args) {
			Command cmd = Context.Command;
			User user = Context.Invoker;

			string resp = Responses.GetResponse(cmd.Module.Name, cmd.Name, user.ResponsePack, index, args);

			Embed response = ResponseBuilder.Message(Context, resp);
			return await SendMessageAsync(response);
		}

		protected async Task<IUserMessage> SendNotOkAsync(int index, params object[] args) {
			Command cmd = Context.Command;
			User user = Context.Invoker;

			string resp = Responses.GetResponse(cmd.Module.Name, cmd.Name, user.ResponsePack, index, args);

			Embed response = ResponseBuilder.Message(Context, resp, false);
			return await SendMessageAsync(response);
		}

		protected Task<SocketUserMessage> NextMessageAsync(ICriterion<SocketUserMessage> criterion,
			TimeSpan? timeout = null) {
			return Interactive.NextMessageAsync(Context, msg => criterion.JudgeAsync(Context, msg), timeout);
		}

		protected Task<bool> TryAddCallbackAsync(IReactionCallback callback, TimeSpan? timeout = null) {
			return Interactive.TryAddCallbackAsync(callback, timeout);
		}

		public void Dispose() {
			Context.Dispose();
		}
	}
}