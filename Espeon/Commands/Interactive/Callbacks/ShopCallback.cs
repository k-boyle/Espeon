using Discord;
using Discord.WebSocket;
using Espeon.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Espeon.Commands
{
    public class ShopCallback : IReactionCallback
    {
        public EspeonContext Context { get; private set; }

        public bool RunOnGatewayThread => false;

        public IUserMessage Message { get; private set; }

        private readonly Emoji _buy = new Emoji("💵");
        private readonly Emoji _leave = new Emoji("❌");

        public IEnumerable<IEmote> Reactions => new[] { _buy, _leave };

        public ICriterion<SocketReaction> Criterion { get; private set; }

        [Inject] private readonly CandyService _candy;
        [Inject] private readonly EmotesService _emotes;
        [Inject] private readonly InteractiveService _interactive;
        [Inject] private readonly MessageService _message;

        public ShopCallback(EspeonContext context, ICriterion<SocketReaction> criterion = null)
        {
            Context = context;
            Criterion = criterion ?? new ReactionFromSourceUser(Context.User.Id);
        }

        public async Task InitialiseAsync()
        {
            Message = await _message.SendAsync(Context, x =>
            {
                x.Embed = ShopEmbed();
            });
        }

        //this is lame but I'm too lazy to implement properly rn
        public async Task<bool> HandleCallbackAsync(SocketReaction reaction)
        {
            var emote = reaction.Emote;

            if(Context.Guild.CurrentUser.GetPermissions(Context.Channel).ManageMessages)
            {
                var user = reaction.User.GetValueOrDefault() 
                    ?? await Context.Client.Rest.GetUserAsync(reaction.UserId);

                await Message.RemoveReactionAsync(emote, user);
            }

            if (emote.Equals(_buy))
            {
                do
                {
                    await _message.SendAsync(Context, x => x.Content = "What would you like to buy?");

                    //var resp = 
                } while (true);
            }
            else if (emote.Equals(_leave))
            {
                await HandleTimeoutAsync();
                return true;
            }

            return false;
        }

        public Task HandleTimeoutAsync()
        {
            if (Context.Guild.CurrentUser.GetPermissions(Context.Channel).ManageMessages)
            {
                return Message.RemoveAllReactionsAsync();
            }

            return Task.CompletedTask;
        }

        private Embed ShopEmbed()
        {
            var builder = new EmbedBuilder
            {
                Title = "Shop",
                Description = "Language packs, language packs! Get your language packs here!",
                Color = Utilities.EspeonColor,
                Author = new EmbedAuthorBuilder
                {
                    IconUrl = Context.User.GetAvatarOrDefaultUrl(),
                    Name = Context.User.GetDisplayName()
                }
            }
                .AddField("Pack", $"owo: 5000{_emotes.Collection["RareCandy"]}");

            return builder.Build();
        }
    }
}
