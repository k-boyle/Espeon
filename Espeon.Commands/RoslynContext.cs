using Disqord;
using Espeon.Core.Entities;
using Espeon.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Espeon.Commands {
	public class RoslynContext {
		public EspeonContext Context { get; }
		public IServiceProvider Services { get; }

		public RoslynContext(EspeonContext context, IServiceProvider services) {
			Context = context;
			Services = services;
		}

		public async Task<IUserMessage> SendAsync(Action<NewMessageProperties> func) {
			var message = Services.GetService<IMessageService>();

			return await message.SendAsync(Context.Message, func);
		}
	}
}