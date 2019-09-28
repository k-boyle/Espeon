using Discord;
using Discord.WebSocket;
using Espeon.Commands;
using Espeon.Services;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Bot.Commands
{
    public sealed class IGuildUserTypeParser : EspeonTypeParser<IGuildUser>
    {
        public override async ValueTask<TypeParserResult<IGuildUser>> ParseAsync(Parameter param, string value, EspeonContext context, IServiceProvider provider)
        {
            var users = context.Guild.Users;

            IGuildUser user = null;
            var id = ParseId(value);

            if (id != 0)
                user = users.FirstOrDefault(x => x.Id == id);

            if (!(user is null))
            {
                var hashIndex = value.LastIndexOf('#');
                if (hashIndex != -1 && hashIndex + 5 == value.Length)
                    user = users.FirstOrDefault(x =>
                        string.Equals(x.Username, value[0..^5], StringComparison.InvariantCultureIgnoreCase) &&
                        string.Equals(x.Discriminator, value.Substring(hashIndex + 1), StringComparison.InvariantCultureIgnoreCase));
            }

            if (!(user is null))
                return new TypeParserResult<IGuildUser>(user);

            IReadOnlyList<SocketGuildUser> matchingUsers = users.Where(x =>
                string.Equals(x.Username, value, StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(x.Nickname, value, StringComparison.InvariantCultureIgnoreCase)).ToImmutableArray();

            var p = context.Invoker.ResponsePack;
            var response = provider.GetService<IResponseService>();

            if (matchingUsers.Count > 1)
                return new TypeParserResult<IGuildUser>(response.GetResponse(this, p, 0));

            if (matchingUsers.Count == 1)
                return new TypeParserResult<IGuildUser>(matchingUsers[0]);

            if (id == 0)
                return new TypeParserResult<IGuildUser>(response.GetResponse(this, p, 1));

            user = await context.Client.Rest.GetGuildUserAsync(context.Guild.Id, id);

            return user is null
                ? new TypeParserResult<IGuildUser>(response.GetResponse(this, p, 1))
                : new TypeParserResult<IGuildUser>(user);
        }

        private static ulong ParseId(string value)
        {
            return value.Length > 3 && value[0] == '<'
                && value[1] == '@'
                && value[^1] == '>'
                && ulong.TryParse(value[2] == '!'
                    ? value[3..^1]
                    : value[2..^1], out var id)
                || ulong.TryParse(value, out id)
                    ? id
                    : 0;
        }
    }
}
