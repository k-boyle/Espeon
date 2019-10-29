using Disqord;
using Espeon.Core;
using Espeon.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Commands {
	public sealed class IMemberTypeParser : EspeonTypeParser<IMember> {
		public override async ValueTask<TypeParserResult<IMember>> ParseAsync(Parameter param, string value,
			EspeonContext context, IServiceProvider provider) {
			IReadOnlyDictionary<Snowflake, CachedMember> members = context.Guild.Members;

			IMember member = null;
			ulong id = ParseId(value);

			if (id != 0) {
				member = members.FirstOrDefault(x => x.Value.Id.RawValue == id).Value;
			}

			if (!(member is null)) {
				int hashIndex = value.LastIndexOf('#');
				if (hashIndex != -1 && hashIndex + 5 == value.Length) {
					member = members.FirstOrDefault(x =>
						string.Equals(x.Value.Name, value[..^5], StringComparison.InvariantCultureIgnoreCase) &&
						string.Equals(x.Value.Discriminator, value.Substring(hashIndex + 1),
							StringComparison.InvariantCultureIgnoreCase)).Value;
				}
			}

			if (!(member is null)) {
				return new TypeParserResult<IMember>(member);
			}

			Dictionary<Snowflake, CachedMember> matchingUsers = members.Where(x =>
					string.Equals(x.Value.Name, value, StringComparison.InvariantCultureIgnoreCase) ||
					string.Equals(x.Value.Nick, value, StringComparison.InvariantCultureIgnoreCase))
				.ToDictionary(x => x.Key, x => x.Value);

			ResponsePack p = context.Invoker.ResponsePack;
			var response = provider.GetService<IResponseService>();

			if (matchingUsers.Count > 1) {
				return new TypeParserResult<IMember>(response.GetResponse(this, p, 0));
			}

			if (matchingUsers.Count == 1) {
				return new TypeParserResult<IMember>(matchingUsers[0]);
			}

			if (id == 0) {
				return new TypeParserResult<IMember>(response.GetResponse(this, p, 1));
			}

			member = await context.Client.GetMemberAsync(context.Guild.Id, id);

			return member is null
				? new TypeParserResult<IMember>(response.GetResponse(this, p, 1))
				: new TypeParserResult<IMember>(member);
		}

		private static ulong ParseId(string value) {
			return value.Length > 3 && value[0] == '<' && value[1] == '@' && value[^1] == '>' &&
			       ulong.TryParse(value[2] == '!' ? value[3..^1] : value[2..^1], out ulong id) ||
			       ulong.TryParse(value, out id)
				? id
				: 0;
		}
	}
}