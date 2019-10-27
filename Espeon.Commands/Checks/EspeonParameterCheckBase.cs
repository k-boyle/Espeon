using Qmmands;
using System;
using System.Threading.Tasks;

namespace Espeon.Commands {
	public abstract class EspeonParameterCheckBase : ParameterCheckAttribute {
		public override ValueTask<CheckResult> CheckAsync(object argument, CommandContext context) {
			return CheckAsync(argument, (EspeonContext) context, context.ServiceProvider);
		}

		public abstract ValueTask<CheckResult> CheckAsync(object argument, EspeonContext context,
			IServiceProvider provider);
	}
}