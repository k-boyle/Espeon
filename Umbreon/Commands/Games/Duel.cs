using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Umbreon.Extensions;
using Umbreon.Helpers;
using Umbreon.Interactive;
using Umbreon.Interactive.Callbacks;
using Umbreon.Interactive.Criteria;
using Umbreon.Interactive.Paginator;
using Umbreon.Interfaces;
using Umbreon.Services;

namespace Umbreon.Commands.Games
{
    public class Duel : IGame, IReactionCallback
    {
        public RunMode RunMode => RunMode.Sync;
        public ICriterion<SocketReaction> Criterion => new EnsureReactionUser(_target.Id);
        public TimeSpan? Timeout => TimeSpan.FromMinutes(2);
        public ICommandContext Context { get; }
        private InteractiveService Interactive { get; }
        private IUserMessage Message { get; set; }

        private readonly MessageService _message;
        private readonly int _challenger;
        private int _defender = -1;
        private readonly Random _random;
        private readonly SocketGuildUser _target;
        private bool _accepted;
        private readonly CandyService _candy;
        private readonly GamesService _game;

        private readonly Emoji _check = new Emoji("✅"), _cross = new Emoji("❌");

        public Duel(ICommandContext context, SocketGuildUser target, int wager, IServiceProvider services)
        {
            Context = context;
            _challenger = wager;
            Interactive = services.GetService<InteractiveService>();
            _message = services.GetService<MessageService>();
            _random = services.GetService<Random>();
            _candy = services.GetService<CandyService>();
            _game = services.GetService<GamesService>();
            _target = target;
        }

        public async Task StartAsync()
        {
            Message = await _message.SendMessageAsync(Context, $"{_target.Mention}", embed: new EmbedBuilder
            {
                Title = "Duel Challenge",
                Description = $"You have been challenged to a duel! React with {_check} to accept, or {_cross} to decline!",
                Color = Color.Red
            }
                .AddField($"Challenger: {(Context.User as IGuildUser).GetDisplayName()}", $"Challenger has wagered {_challenger}{EmotesHelper.Emotes["rarecandy"]} rare candies")
                .Build());

            await Message.AddReactionsAsync(new RequestOptions
                {
                    BypassBuckets = true
                },
                _check,
                _cross);

            Interactive.AddReactionCallback(Message, this);

            _ = Task.Delay(Timeout.GetValueOrDefault()).ContinueWith(_ =>
            {
                Interactive.RemoveReactionCallback(Message);
                if (!_accepted)
                {
                    _ = _message.NewMessageAsync(Context, $"{_target.Mention} you didn't respond in time");
                }
                _ = Message.RemoveAllReactionsAsync();
                _game.LeaveGame(Context.User.Id);
            });
        }

        public async Task<bool> HandleCallbackAsync(SocketReaction reaction)
        {
            var emoji = reaction.Emote;

            if (emoji.Equals(_check))
            {
                _accepted = true;
                await Message.RemoveAllReactionsAsync(new RequestOptions
                {
                    BypassBuckets = true
                });
                Interactive.RemoveReactionCallback(Message);

                _ = Task.Run(async () =>
                {
                    await _message.NewMessageAsync(Context, $"{_target.GetDisplayName()} please respond with your defending wager. The winner is randomly decided but weighted " +
                                                            "more towards whoever holds the larger wager");

                    while(_defender < 0)
                    {
                        var criteria = new Criteria<SocketMessage>()
                            .AddCriterion(new EnsureSourceChannelCriterion())
                            .AddCriterion(new EnsureFromUserCriterion(reaction.UserId))
                            .AddCriterion(new EnsureIsIntegerCriterion());

                        var response = await Interactive.NextMessageAsync(Context, criteria, TimeSpan.FromMinutes(1));

                        if (int.TryParse(response.Content, out _defender))
                        {
                            if (_defender > _candy.GetCandies(_target.Id))
                            {
                                _defender = -1;
                                await _message.NewMessageAsync(Context,
                                    $"You only have {_candy.GetCandies(_target.Id)}{EmotesHelper.Emotes["rarecandy"]} candies, try again");
                                continue;
                            }

                            if (_defender >= 0)
                            {
                                _ = EndAsync();
                                return;
                            }
                        }

                        await _message.NewMessageAsync(Context, "Please enter a positive integer");
                    }
                });
            }

            if (!emoji.Equals(_cross)) return true;
            _accepted = true;
            _ = EndAsync();

            return true;
        }

        public async Task EndAsync()
        {
            if (!_accepted)
            {
                await _message.NewMessageAsync(Context, $"{(Context.User as IGuildUser).GetDisplayName()} {_target.GetDisplayName()} has declined your wager");
                return;
            }

            var total = _challenger + _defender;
            var winner = _random.Next(total) < _challenger;

            await _message.NewMessageAsync(Context, $"{(winner ? Context.User.Mention : _target.Mention)} you win the wager! You win {(winner ? _defender : _challenger)}{EmotesHelper.Emotes["rarecandy"]} rare candies!");
            _candy.UpdateCandies(Context.User.Id, false, winner ? _challenger : -_challenger);
            _candy.UpdateCandies(_target.Id, false, winner ? -_defender : _defender);
            _game.LeaveGame(Context.User.Id);
        }
    }
}
