using Discord;
using Discord.WebSocket;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Commands
{
    public sealed class IGuildUserTypeParser : TypeParser<IGuildUser>
    {
        public override async ValueTask<TypeParserResult<IGuildUser>> ParseAsync(Parameter param, string value, CommandContext ctx, IServiceProvider provider)
        {
            var context = (EspeonContext)ctx;

            var users = context.Guild.Users;

            IGuildUser user = null;
            var id = ParseId(value);

            if (id != 0)
                user = users.FirstOrDefault(x => x.Id == id);

            if (user == null)
            {
                var hashIndex = value.LastIndexOf('#');
                if (hashIndex != -1 && hashIndex + 5 == value.Length)
                    user = users.FirstOrDefault(x =>
                        string.Equals(x.Username, value[0..^5], StringComparison.InvariantCultureIgnoreCase) &&
                        string.Equals(x.Discriminator, value.Substring(hashIndex + 1), StringComparison.InvariantCultureIgnoreCase));
            }

            if (user != null)
                return new TypeParserResult<IGuildUser>(user);

            IReadOnlyList<SocketGuildUser> matchingUsers = context.Guild != null
                ? users.Where(x => string.Equals(x.Username, value, StringComparison.InvariantCultureIgnoreCase)
                    || string.Equals(x.Nickname, value, StringComparison.InvariantCultureIgnoreCase)).ToImmutableArray()
                : users.Where(x => string.Equals(x.Username, value, StringComparison.InvariantCultureIgnoreCase)).ToImmutableArray();

            var resp = new Dictionary<ResponsePack, string[]>
            {
                [ResponsePack.Default] = new[]
                {
                    "Multiple users found, try mentioning them",
                    "Failed to find a matching user"
                },
                [ResponsePack.owo] = new []
                {
                    "i fwound twoo many pweopol, twy mentioning them :3",
                    "fwailed to fwind user"
                }
            };

            var p = context.Invoker.ResponsePack;

            if (matchingUsers.Count > 1)
                return new TypeParserResult<IGuildUser>(resp[p][0]);

            if (matchingUsers.Count == 1)
                user = matchingUsers[0];

            if (!(user is null))
                return new TypeParserResult<IGuildUser>(user);

            if(id == 0)
                return new TypeParserResult<IGuildUser>(resp[p][1]);

            user = await context.Client.Rest.GetGuildUserAsync(context.Guild.Id, id);

            return user is null
                ? new TypeParserResult<IGuildUser>(resp[p][1])
                : new TypeParserResult<IGuildUser>(user);
        }

        private ulong ParseId(string value)
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
