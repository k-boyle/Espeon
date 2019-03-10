using Discord;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Espeon.Commands
{
    public class ShopCallback : IReactionCallback
    {
        public EspeonContext Context { get; private set; }

        public IUserMessage Message { get; private set; }

        public IEnumerable<IEmote> Reactions => new[] { new Emoji("💵"), new Emoji("❌") };

        public ICriterion<SocketReaction> Criterion { get; private set; }

        public ShopCallback(EspeonContext context, ICriterion<SocketReaction> criterion = null)
        {
            Context = context;
            Criterion = criterion ?? new ReactionFromSourceUser(Context.User.Id);
        }

        public async Task InitialiseAsync()
        {

        }

        public Task<bool> HandleCallbackAsync(SocketReaction reaction)
        {
            throw new System.NotImplementedException();
        }

        public Task HandleTimeoutAsync()
        {
            throw new System.NotImplementedException();
        }

        private Embed ShopEmbed()
        {
            var builder = new EmbedBuilder
            {
                Title = "Shop",
                Description = "Language packs, language packs! Get your language packs here!"
            };

            return builder.Build();
        }
    }
}
