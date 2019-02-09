using Discord.WebSocket;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Commands.TypeParsers
{
    public sealed class SocketGuildUserParser : TypeParser<SocketGuildUser>
    {
        public override Task<TypeParserResult<SocketGuildUser>> ParseAsync(string value, ICommandContext ctx,
            IServiceProvider provider)
        {
            var context = ctx as EspeonContext;

            var users = context!.Guild.Users;
            
            SocketGuildUser user = null;
            if (value.Length > 3 && value[0] == '<' && value[1] == '@' && value[^1] == '>' &&
                ulong.TryParse(value[2] == '!' ? value[3..^1] : value[2..^1],
                    out var id)
                || ulong.TryParse(value, out id))
                user = users.FirstOrDefault(x => x.Id == id);

            if (user == null)
            {
                var hashIndex = value.LastIndexOf('#');
                if (hashIndex != -1 && hashIndex + 5 == value.Length)
                    user = users.FirstOrDefault(x =>
                        x.Username == value[0..^5] &&
                        x.Discriminator == value.Substring(hashIndex + 1));
            }

            if (user != null)
                return Task.FromResult(new TypeParserResult<SocketGuildUser>(user));

            IReadOnlyList<SocketGuildUser> matchingUsers = context.Guild != null
                ? users.Where(x => x.Username == value || x.Nickname == value).ToImmutableArray()
                : users.Where(x => x.Username == value).ToImmutableArray();

            if (matchingUsers.Count > 1)
                return Task.FromResult(
                    new TypeParserResult<SocketGuildUser>("Multiple matches found. Mention the user or use their ID."));

            if (matchingUsers.Count == 1)
                user = matchingUsers[0];

            return user == null
                ? Task.FromResult(new TypeParserResult<SocketGuildUser>("No user found matching the input."))
                : Task.FromResult(new TypeParserResult<SocketGuildUser>(user));
        }
    }
}
