using Discord;
using Discord.WebSocket;
using Espeon.Services;
using Microsoft.Extensions.DependencyInjection;
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
                        x.Discriminator == value.Substring(hashIndex + 1));
            }

            if (user != null)
                return new TypeParserResult<IGuildUser>(user);

            IReadOnlyList<SocketGuildUser> matchingUsers = users.Where(x =>
                string.Equals(x.Username, value, StringComparison.InvariantCultureIgnoreCase)
                || string.Equals(x.Nickname, value, StringComparison.InvariantCultureIgnoreCase)).ToImmutableArray();
            
            var p = context.Invoker.ResponsePack;
            var response = provider.GetService<ResponseService>();
            
            switch (matchingUsers.Count)
            {
                case 0:
                    return TypeParserResult<IGuildUser>.Unsuccessful(response.GetResponse(this, p, 1));
                case 1:
                    return TypeParserResult<IGuildUser>.Successful(matchingUsers[0]);
                default:
                    matchingUsers = matchingUsers.Where(x => x.Username == value || x.Nickname == value).ToArray();
                    if (matchingUsers.Count == 1)
                        return TypeParserResult<IGuildUser>.Successful(matchingUsers[0]);

                    return TypeParserResult<IGuildUser>.Unsuccessful(response.GetResponse(this, p, 0));
            }
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
