using Disqord;
using Espeon.Core.Database;
using Espeon.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Commands {
	public sealed class CachedRoleTypeParser : EspeonTypeParser<CachedRole> {
		public override ValueTask<TypeParserResult<CachedRole>> ParseAsync(Parameter param, string value,
			EspeonContext context, IServiceProvider provider) {
			CachedRole role = null;

			if (value.Length > 3 && value[0] == '<' && value[1] == '@' && value[2] == '&' && value[^1] == '>' &&
			    ulong.TryParse(value[3..^1], out ulong id) || ulong.TryParse(value, out id)) {
				role = context.Guild.Roles.FirstOrDefault(x => x.Value.Id == id).Value;
			}

			role ??= context.Guild.Roles.FirstOrDefault(x =>
				string.Equals(x.Value.Name, value, StringComparison.InvariantCultureIgnoreCase)).Value;

			var response = provider.GetService<IResponseService>();
			User user = context.Invoker;

			return role is null
				? new TypeParserResult<CachedRole>(response.GetResponse(this, user.ResponsePack, 0))
				: new TypeParserResult<CachedRole>(role);
		}
	}
}