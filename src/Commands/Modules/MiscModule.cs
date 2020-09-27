using Disqord;
using Disqord.Bot;
using Disqord.Rest;
using Qmmands;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using static Espeon.LocalisationStringKey;

namespace Espeon {
    [Name("Misc Commands")]
    [Description("Commands that doesn't really fit into a specific category")]
    public class MiscModule : EspeonCommandModule {
        public HttpClient Client { get; set; }
        
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

            if (string.IsNullOrWhiteSpace(message.Content)) {
                await ReplyAsync(MESSAGE_HAS_EMPTY_CONTENT);
                return;
            }
            
            await webhookClient.ExecuteAsync(
                Mock(message.Content),
                name: Context.Guild.Members.TryGetValue(message.Author.Id, out var member)
                    ? member.DisplayName
                    : message.Author.Name,
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
                    Name = Context.Guild.Members.TryGetValue(author.Id, out var member) ? member.DisplayName : author.Name,
                    Url = message.GetJumpUrl(Context.Bot.GetChannel(message.ChannelId) is CachedTextChannel channel ? channel.Guild : null)
                },
                Timestamp = message.CreatedAt,
                ImageUrl = message is IUserMessage userMessage
                    ? userMessage.Attachments.FirstOrDefault() is { } attachment ? attachment.Url : null
                    : null
            };

            await ReplyAsync(embed: builder.Build());
        }
        
        [Command("<a:pepohyperwhatif:715291110297043005>")]
        public async Task PepoWhatIfAsync() {
            await ReplyAsync("<a:pepohyperwhatif:715291110297043005>");
        }
        
        [Command("emote")]
        [RequireBotGuildPermissions(Permission.ManageEmojis)]
        [RequireMemberGuildPermissions(Permission.ManageEmojis)]
        public async Task StealEmoteAsync(LocalCustomEmoji emoji, string name = null) {
            await using var httpStream = await Client.GetStreamAsync(emoji.GetUrl());
            await using var memStream = new MemoryStream();
            await httpStream.CopyToAsync(memStream);
            memStream.Position = 0;
            var created = await Context.Guild.CreateEmojiAsync(memStream, name ?? emoji.Name);
            await ReplyAsync(created.ToString());
        }
    }
}