using Discord;
using Discord.WebSocket;
using Espeon.Databases;
using Espeon.Databases.GuildStore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using Casino.Common.DependencyInjection;
using Casino.Common.Discord.Net;

namespace Espeon.Services
{
    public class StarboardService : BaseService<InitialiseArgs>
    {
        [Inject] private readonly DiscordSocketClient _client;
        [Inject] private readonly IServiceProvider _services;

        private static Emoji Star => Utilities.Star;

        public StarboardService(IServiceProvider services) : base(services)
        {
            _client.ReactionAdded += ReactionAddedAsync;
            _client.ReactionRemoved += ReactionRemovedAsync;
        }

        private async Task ReactionAddedAsync(Cacheable<IUserMessage, ulong> msg, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (!(channel is SocketTextChannel textChannel))
                return;

            if (!reaction.Emote.Equals(Star))
                return;

            var message = await msg.GetOrDownloadAsync();

            if (reaction.UserId == message.Author.Id)
                return;

            using var guildStore = _services.GetService<GuildStore>();
            var guild = await guildStore.GetOrCreateGuildAsync(textChannel.Guild, x => x.StarredMessages);

            if (!(textChannel.Guild.GetTextChannel(guild.StarboardChannelId) is SocketTextChannel starChannel))
                return;

            var count = message.Reactions[Star].ReactionCount;

            var flat = await message.GetReactionUsersAsync(Star, count).FlattenAsync();
            var users = flat.Where(x => x.Id == message.Author.Id).ToArray();

            count = users.Length;

            if (count < guild.StarLimit)
                return;

            var foundMessage = guild.StarredMessages
                .FirstOrDefault(x => x.Id == message.Id || x.StarboardMessageId == message.Id);

            var m = $"{Star} **{count}** - {(message.Author as IGuildUser).GetDisplayName()} in <#{message.Channel.Id}>";

            if (foundMessage is null)
            {
                var embed = Utilities.BuildStarMessage(message);

                var newStar = await starChannel.SendMessageAsync(m, embed: embed);

                guild.StarredMessages.Add(new StarredMessage
                {
                    AuthorId = message.Author.Id,
                    ChannelId = message.Channel.Id,
                    Id = message.Id,
                    StarboardMessageId = newStar.Id,
                    ReactionUsers = users.Select(x => x.Id).ToList(),
                    ImageUrl = embed.Image?.Url,
                    Content = message.Content
                });

                guildStore.Update(guild);

                await guildStore.SaveChangesAsync();
            }
            else
            {
                if (foundMessage.ReactionUsers.Contains(reaction.UserId))
                    return;

                foundMessage.ReactionUsers.Add(reaction.UserId);

                if (await starChannel.GetMessageAsync(foundMessage.StarboardMessageId) is IUserMessage fetchedMessage)
                    await fetchedMessage.ModifyAsync(x => x.Content = m);

                guildStore.Update(guild);

                await guildStore.SaveChangesAsync();
            }
        }

        private async Task ReactionRemovedAsync(Cacheable<IUserMessage, ulong> msg, ISocketMessageChannel channel, SocketReaction reaction)
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

            if (foundMessage is null)
                return;

            if (!foundMessage.ReactionUsers.Remove(reaction.UserId))
                return;

            var count = message.Reactions.ContainsKey(Star) ? message.Reactions[Star].ReactionCount : 0;

            var starMessage = await starChannel.GetMessageAsync(foundMessage.StarboardMessageId) as IUserMessage;

            if (starMessage is null || count < guild.StarLimit)
            {
                _ = starMessage?.DeleteAsync();

                guild.StarredMessages.Remove(foundMessage);
            }
            else
            {
                var m = $"{Star} **{count}** - {(message.Author as IGuildUser).GetDisplayName()} in <#{message.Channel.Id}>";

                await starMessage.ModifyAsync(x => x.Content = m);
            }

            guildStore.Update(guild);

            await guildStore.SaveChangesAsync();
        }
    }
}
