using Discord;
using Discord.Net.Helpers;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;
using Umbreon.Core.Models.Database;

namespace Umbreon.Services
{
    public class StarboardService
    {
        private readonly DatabaseService _database;
        private readonly DiscordSocketClient _client;

        public StarboardService(DatabaseService database, DiscordSocketClient client)
        {
            _database = database;
            _client = client;
        }

        public void Initialize()
        {
            _client.ReactionAdded += StarboardReactionAdded;
        }

        private async Task StarboardReactionAdded(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (!string.Equals(reaction.Emote.Name, "⭐", StringComparison.CurrentCultureIgnoreCase)) return;

            if (channel is SocketGuildChannel guildChannel)
            {
                var guild = _database.GetGuild(guildChannel.Guild.Id);
                var starboard = guild.Starboard;
                if (!starboard.Enabled) return;
                if (_client.GetChannel(starboard.ChannelId) is SocketTextChannel starChannel)
                {
                    if (channel.Id == starChannel.Id) return;

                    var msg = await message.GetOrDownloadAsync();
                    var starCount = msg.Reactions[new Emoji("⭐")].ReactionCount;
                    if (starCount >= starboard.StarLimit)
                    {
                        if (!starboard.StarredMessages.Select(x => x.MessageId).Contains(msg.Id))
                        {
                            var newStarMsg = await starChannel.SendMessageAsync($"<>\n⭐'s {starCount}", embed: BuildEmbed(msg)); // TODO hope my pr gets merged for URL here

                            var newStar = new StarredMessage
                            {
                                MessageId = msg.Id,
                                StarCount = starCount,
                                StarMessageId = newStarMsg.Id
                            };
                            starboard.StarredMessages.Add(newStar);
                            guild.Starboard = starboard;
                            _database.UpdateGuild(guild);
                            return;
                        }

                        var targetStar = starboard.StarredMessages.FirstOrDefault(x => x.MessageId == msg.Id);
                        targetStar.StarCount = starCount;
                        starboard.StarredMessages.Find(x => x == targetStar).StarCount = starCount;
                        guild.Starboard = starboard;
                        _database.UpdateGuild(guild);
                        var fetchedMessage = await starChannel.GetMessageAsync(targetStar.StarMessageId);
                        await (fetchedMessage as IUserMessage).ModifyAsync(x => x.Embed = BuildEmbed(msg));
                    }
                }
            }
        }

        private static Embed BuildEmbed(IMessage msg)
        {
            return new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    IconUrl = msg.Author.GetAvatarOrDefaultUrl(),
                    Name = (msg.Author as SocketGuildUser).GetDisplayName()
                },
                Description = msg.Content,
                Color = Color.Gold,
                Timestamp = DateTimeOffset.UtcNow
            }.Build();
        }
    }
}
