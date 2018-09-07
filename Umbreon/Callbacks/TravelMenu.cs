using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbreon.Helpers;
using Umbreon.Interactive;
using Umbreon.Interactive.Callbacks;
using Umbreon.Interactive.Criteria;
using Umbreon.Interactive.Paginator;
using Umbreon.Services;
using Colour = Discord.Color;

namespace Umbreon.Callbacks
{
    public class TravelMenu : IReactionCallback
    {
        public RunMode RunMode => RunMode.Sync;
        public ICriterion<SocketReaction> Criterion => new EnsureReactionFromSourceUserCriterion();
        public TimeSpan? Timeout => TimeSpan.FromMinutes(2);
        public ICommandContext Context { get; }
        public InteractiveService Interactive { get; }

        private readonly CandyService _candy;
        private readonly MessageService _message;
        private readonly PokemonPlayerService _player;

        private readonly Dictionary<Emoji, int> _emojis = new Dictionary<Emoji, int>
        {
            { new Emoji("1⃣"), 1 },
            { new Emoji("2⃣"), 2 },
            { new Emoji("3⃣"), 3 },
            { new Emoji("4⃣"), 4 },
            { new Emoji("5⃣"), 5 },
            { new Emoji("6⃣"), 6 },
            { new Emoji("7⃣"), 7 },
            { new Emoji("8⃣"), 8 },
            { new Emoji("9⃣"), 9 }
        };

        public IUserMessage Message;

        public TravelMenu(ICommandContext context, IServiceProvider services)
        {
            Context = context;
            Interactive = services.GetService<InteractiveService>();
            _candy = services.GetService<CandyService>();
            _message = services.GetService<MessageService>();
            _player = services.GetService<PokemonPlayerService>();
        }

        public async Task DisplayAsync()
        {
            Message = await _message.SendMessageAsync(Context, string.Empty, embed: BuildEmbed());

            _ = Task.Run(async () =>
            {
                foreach (var emoji in _emojis)
                    await Message.AddReactionAsync(emoji.Key, new RequestOptions
                    {
                        BypassBuckets = true
                    });
            });

            Interactive.AddReactionCallback(Message, this);
            _ = Task.Delay(Timeout.GetValueOrDefault()).ContinueWith(_ =>
            {
                Interactive.RemoveReactionCallback(Message);
                _ = Message.RemoveAllReactionsAsync();
            });
        }

        public async Task<bool> HandleCallbackAsync(SocketReaction reaction)
        {
            if (!(reaction.Emote is Emoji emoji)) return false;
            if (!_emojis.Any(x => Equals(x.Key, emoji))) return false;

            var time = _player.GetTravel(Context.User.Id).ToUniversalTime().AddMinutes(10);

            if (time > DateTime.UtcNow)
            {
                await Message.ModifyAsync(x =>
                {
                    x.Content = "You have already recently travelled";
                    x.Embed = null;
                });
                await Message.RemoveAllReactionsAsync();
                Interactive.RemoveReactionCallback(Message);
                return true;
            }

            if (_emojis[emoji] == _player.GetHabitat(Context.User.Id))
            {
                await _message.NewMessageAsync(Context, "You are already in this habitat");
                _ = Message.RemoveReactionAsync(emoji, Context.User);
                return false;
            }

            if (_emojis[emoji] == 5)
            {
                if (_candy.GetCandies(Context.User.Id) < 10)
                {
                    await _message.NewMessageAsync(Context, "You don't have enough candies to enter this zone");
                    _ = Message.RemoveReactionAsync(emoji, Context.User);
                    return false;
                }
            }

            _player.SetArea(Context.User.Id, _emojis[emoji]);
            await Message.ModifyAsync(x =>
            {
                x.Embed = null;
                x.Content = $"You are now in {_player.GetHabitats()[_emojis[emoji]]} area";
            });
            await Message.RemoveAllReactionsAsync();
            Interactive.RemoveReactionCallback(Message);
            return true;

        }

        private Embed BuildEmbed()
        {
            var builder = new EmbedBuilder
            {
                Color = Colour.Gold,
                Title = "Areas"
            };

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Choose what area you want to travel to");

            foreach (var habit in _player.GetHabitats())
            {
                if (habit.Key == 5)
                {
                    stringBuilder.AppendLine($"{_emojis.FirstOrDefault(x => x.Value == habit.Key).Key}: {habit.Value} - 10{EmotesHelper.Emotes["rarecandy"]} rare candies");
                    continue;
                }

                stringBuilder.AppendLine($"{_emojis.FirstOrDefault(x => x.Value == habit.Key).Key}: {habit.Value}");
            }

            builder.WithDescription(stringBuilder.ToString());

            return builder.Build();
        }
    }
}
