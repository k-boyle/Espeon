using Disqord;
using Disqord.Rest;
using Qmmands;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon {
    [Name("Misc Commands")]
    [Description("Commands that doesn't really fit into a specific category")]
    public class MiscModule : EspeonCommandModule {
        [Name("Mock")]
        [Description("Mocks a user")]
        [Command("mock", "m")]
        public async Task MockAsync(IMessage message) {
            var webhooks = await Context.Channel.GetWebhooksAsync();
            var webhook = webhooks.FirstOrDefault() ?? await Context.Channel.CreateWebhookAsync("Espeon Webhook");
            var webhookClient = new RestWebhookClient(webhook);

            static string Mock(string inStr) {
                return string.Create(inStr.Length, inStr, (span, state) => {
                    for (int i = span.Length - 1; i >= 0; i--) {
                        span[i] = (i & 1) == 0 ? state[i] : char.ToUpper(state[i]);
                    }
                });
            }

            await webhookClient.ExecuteAsync(
                Mock(message.Content),
                name: Context.Guild.Members[message.Author.Id].DisplayName,
                avatarUrl: message.Author.GetAvatarUrl());
        }
        
        [Name("Quote")]
        [Description("Quote a message")]
        [Command("quote", "q")]
        public async Task QuoteAsync(IMessage message) {
            var author = message.Author;
            var builder = new LocalEmbedBuilder {
                Color = Constants.EspeonColour,
                Description = message.Content,
                Author = new LocalEmbedAuthorBuilder {
                    IconUrl = author.GetAvatarUrl(),
                    Name = Context.Guild.Members[author.Id] is { } member ? member.DisplayName : author.Name,
                    Url = message.GetJumpUrl(Context.Bot.GetChannel(message.ChannelId) is CachedTextChannel channel ? channel.Guild : null)
                },
                Timestamp = message.CreatedAt,
                ImageUrl = message is IUserMessage userMessage
                    ? userMessage.Attachments.FirstOrDefault() is { } attachment ? attachment.Url : null
                    : null
            };

            await ReplyAsync(embed: builder.Build());
        }
    }
}