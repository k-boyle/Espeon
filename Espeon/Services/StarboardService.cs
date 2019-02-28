using Discord;
using Discord.WebSocket;
using Espeon.Databases.CommandStore;
using Espeon.Databases.GuildStore;
using Espeon.Databases.UserStore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Services
{
    public class StarboardService : BaseService
    {
        [Inject] private readonly DiscordSocketClient _client;
        [Inject] private readonly IServiceProvider _services;

        private readonly static Emoji Star = new Emoji("⭐");

        public override Task InitialiseAsync(UserStore userStore, GuildStore guildStore, CommandStore commandStore, IServiceProvider services)
        {
            _client.ReactionAdded += ReactionAddedAsync;

            return Task.CompletedTask;
        }

        private async Task ReactionAddedAsync(Cacheable<IUserMessage, ulong> msg, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (!(channel is SocketTextChannel textChannel))
                return;

            if (!reaction.Emote.Equals(Star))
                return;

            using var guildStore = _services.GetService<GuildStore>();
            var guild = await guildStore.GetOrCreateGuildAsync(textChannel.Guild, x => x.StarredMessages);

            if (!(textChannel.Guild.GetTextChannel(guild.StarboardChannelId) is SocketTextChannel starChannel))
                return;

            var message = await msg.GetOrDownloadAsync();

            var foundMessage = guild.StarredMessages
                .FirstOrDefault(x => x.Id == message.Id || x.StarboardMessageId == message.Id);

            if(foundMessage is null)
            {
                var newStar = await starChannel.SendMessageAsync(string.Empty, embed: new EmbedBuilder
                {
                    Author = new EmbedAuthorBuilder
                    {
                        Name = (message.Author as IGuildUser).GetDisplayName(),
                        IconUrl = message.Author.GetAvatarOrDefaultUrl()
                    },
                    Description = message.Content,
                    
                }
                    .Build());
            }
        }
    }
}
