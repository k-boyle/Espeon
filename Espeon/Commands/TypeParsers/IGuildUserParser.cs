using Discord;
using Discord.WebSocket;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Commands.TypeParsers
{
    public sealed class IGuildUserTypeParser : TypeParser<IGuildUser>
    {
        public override async Task<TypeParserResult<IGuildUser>> ParseAsync(string value, ICommandContext ctx, IServiceProvider provider)
        {
            var context = ctx as EspeonContext;

            var users = context!.Guild.Users;

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

            if (matchingUsers.Count > 1)
                return new TypeParserResult<IGuildUser>("Multiple matches found. Mention the user or use their ID.");

            if (matchingUsers.Count == 1)
                user = matchingUsers[0];

            if (!(user is null))
                return new TypeParserResult<IGuildUser>(user);

            user = await context.Client.Rest.GetGuildUserAsync(context.Guild.Id, id);

            if (user is null)
                return new TypeParserResult<IGuildUser>("Failed to find a matching user");

            return new TypeParserResult<IGuildUser>(user);
        }

        private ulong ParseId(string value)
        {
            return value.Length > 3 
                && value[0] == '<' 
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
