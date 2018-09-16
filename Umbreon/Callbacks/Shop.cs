using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Umbreon.Attributes;
using Umbreon.Helpers;
using Umbreon.Interactive;
using Umbreon.Interactive.Callbacks;
using Umbreon.Interactive.Criteria;
using Umbreon.Interactive.Paginator;
using Umbreon.Services;
using Colour = Discord.Color;

namespace Umbreon.Callbacks
{
    public class Shop : IReactionCallback
    {
        public RunMode RunMode => RunMode.Sync;
        public ICriterion<SocketReaction> Criterion => new EnsureReactionFromSourceUserCriterion();
        public TimeSpan? Timeout => TimeSpan.FromMinutes(2);
        public ICommandContext Context { get; }

        private static IEnumerable<Type> Items => AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
            .Where(y => y.GetCustomAttributes(typeof(ShopItemAttribute), true).Length > 0)
            .OrderBy(z => z.GetCustomAttributes().OfType<ShopItemAttribute>().FirstOrDefault()?.Price);

        private IUserMessage _message;
        private readonly MessageService _messageService;
        private readonly InteractiveService _interactive;
        private readonly CandyService _candy;
        private readonly PokemonPlayerService _player;
        private readonly IEmote _cross = new Emoji("❌");

        public Shop(ICommandContext context, IServiceProvider services)
        {
            Context = context;
            _messageService = services.GetService<MessageService>();
            _interactive = services.GetService<InteractiveService>();
            _candy = services.GetService<CandyService>();
            _player = services.GetService<PokemonPlayerService>();
        }

        public async Task DisplayAsync()
        {
            _message = await _messageService.SendMessageAsync(Context, string.Empty, embed: BuildEmbed());

            _ = Task.Run(async () =>
            {
                foreach (var item in Items)
                {
                    var attr = item.GetCustomAttributes().OfType<ShopItemAttribute>().FirstOrDefault();

                    if(await _candy.GetCandiesAsync(Context.User.Id) < attr?.Price)
                        continue;

                    await _message.AddReactionAsync(attr?.Emote, new RequestOptions
                    {
                        BypassBuckets = true
                    });
                }

                await _message.AddReactionAsync(_cross);
            });

            _interactive.AddReactionCallback(_message, this);

            _ = Task.Delay(Timeout.GetValueOrDefault()).ContinueWith(_ =>
            {
                _ = _message.RemoveAllReactionsAsync();
                _interactive.RemoveReactionCallback(_message);
            });
        }

        private Embed BuildEmbed()
        {
            var builder = new EmbedBuilder
            {
                Color = Colour.Blue,
                Title = "Shop",
                ThumbnailUrl = "http://pm1.narvii.com/6864/f2c56e767052744c5b03b99c246539dee1d667bdr1-384-384v2_00.jpg"
            };
            
            var sb = new StringBuilder();

            sb.AppendLine($"Candies: {_candy.GetCandiesAsync(Context.User.Id)}{EmotesHelper.Emotes["rarecandy"]}");
            sb.AppendLine("");

            foreach(var item in Items)
            {
                var attr = item.GetCustomAttributes().OfType<ShopItemAttribute>().FirstOrDefault();
                sb.AppendLine($"{attr?.Emote}{attr?.ItemName} - {attr?.Price}{EmotesHelper.Emotes["rarecandy"]}");
            }

            builder.Description = sb.ToString();

            return builder.Build();
        }

        public async Task<bool> HandleCallbackAsync(SocketReaction reaction)
        {
            var emote = reaction.Emote;
            _ = _message.RemoveReactionAsync(emote, Context.User);

            if (emote.Equals(_cross))
            {
                _interactive.RemoveReactionCallback(_message);
                await _message.RemoveAllReactionsAsync();
                return true;
            }

            var shopItems = Items.Select(x => x.GetCustomAttributes().OfType<ShopItemAttribute>().FirstOrDefault());
            var item = shopItems.FirstOrDefault(x => x.Emote.Equals(emote));

            if (item is null) return false;
            if (await _candy.GetCandiesAsync(Context.User.Id) < item.Price)
            {
                await _messageService.NewMessageAsync(Context, "You don't have enough candies to afford this");
                await _message.RemoveReactionAsync(item.Emote, Context.Client.CurrentUser);
                return false;
            }

            await _candy.UpdateCandiesAsync(Context.User.Id, false, -item.Price);

            await _player.AddItemAsync(Context.User.Id, item);

            await _message.ModifyAsync(x => x.Embed = BuildEmbed());

            return false;
        }
    }
}
