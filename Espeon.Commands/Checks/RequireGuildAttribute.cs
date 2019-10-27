using Espeon.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Espeon.Commands {
	public class RequireGuildAttribute : EspeonCheckBase {
		private readonly ulong _id;

		public RequireGuildAttribute(ulong id) {
			this._id = id;
		}

		public override ValueTask<CheckResult> CheckAsync(EspeonContext context, IServiceProvider provider) {
			var response = provider.GetService<IResponseService>();

			return context.Guild.Id == this._id
				? CheckResult.Successful
				: CheckResult.Unsuccessful(response.GetResponse(this, context.Invoker.ResponsePack, 0));
		}
	}
}