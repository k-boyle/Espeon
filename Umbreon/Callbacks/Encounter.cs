using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Umbreon.Core.Entities.Pokemon;
using Umbreon.Core.Entities.Pokemon.Pokeballs;
using Umbreon.Core.Entities.User;
using Umbreon.Extensions;
using Umbreon.Helpers;
using Umbreon.Interactive;
using Umbreon.Interactive.Callbacks;
using Umbreon.Interactive.Criteria;
using Umbreon.Interactive.Paginator;
using Umbreon.Services;

namespace Umbreon.Callbacks
{
    public class Encounter : IReactionCallback
    {
        public RunMode RunMode => RunMode.Sync;
        public ICriterion<SocketReaction> Criterion => new EnsureReactionFromSourceUserCriterion();
        public TimeSpan? Timeout => TimeSpan.FromMinutes(2);
        public ICommandContext Context { get; }

        private readonly PokemonData _encounter;
        private readonly ulong _playerId;
        private UserObject _user => _player.GetCurrentPlayer(_playerId);
        private readonly InteractiveService _interactive;
        private readonly MessageService _messageService;
        private readonly PokemonPlayerService _player;
        private readonly PokemonDataService _data;
        private IUserMessage _message;
        private Random _random;

        private bool caught;

        public Encounter(ICommandContext context, PokemonData encounter, ulong playerId, IServiceProvider services)
        {
            Context = context;
            _encounter = encounter;
            _playerId = playerId;
            _interactive = services.GetService<InteractiveService>();
            _messageService = services.GetService<MessageService>();
            _player = services.GetService<PokemonPlayerService>();
            _data = services.GetService<PokemonDataService>();
            _random = services.GetService<Random>();
        }

        public async Task SetupAsync()
        {
            _message = await _messageService.SendFileAsync(Context, PokemonDataService.GetImage(_encounter),
                embed: BuildEmbed());

            _ = Task.Run(async () =>
            {
                await _message.AddReactionAsync(EmotesHelper.Emotes["pokeball"], new RequestOptions
                {
                    BypassBuckets = true
                });
                await _message.AddReactionAsync(EmotesHelper.Emotes["greatball"], new RequestOptions
                {
                    BypassBuckets = true
                });
                await _message.AddReactionAsync(EmotesHelper.Emotes["ultraball"], new RequestOptions
                {
                    BypassBuckets = true
                });
            });

            _interactive.AddReactionCallback(_message, this);

            _ = Task.Delay(Timeout.GetValueOrDefault()).ContinueWith(_ =>
            {
                _interactive.RemoveReactionCallback(_message);
                _ = _message.RemoveAllReactionsAsync();
                if(!caught)
                    _ = _messageService.NewMessageAsync(Context, $"The wild {_encounter.Name} escaped");
            });

        }

        private Embed BuildEmbed()
        {
            var builder = new EmbedBuilder
            {
                Title = "A wild encounter!",
                Color = _data.GetColour(_encounter), 
                ThumbnailUrl = "attachment://image.png"
            };

            var balls = _user.Bag.PokeBalls;

            builder.AddField($"A {_encounter.Name.FirstLetterToUpper()} appeared!", 
                $"Catch rate: {_encounter.CaptureRate}\n" +
                $"Encounter rate: {_encounter.EncounterRate}", 
                true);

            builder.AddField("Bag", 
                $"{EmotesHelper.Emotes["pokeball"]} Pokeballs: {balls.Count(x => x is NormalBall)}\n" +
                $"{EmotesHelper.Emotes["greatball"]} Greatballs: {balls.Count(x => x is GreatBall)}\n" +
                $"{EmotesHelper.Emotes["ultraball"]} Ultaballs: {balls.Count(x => x is UltraBall)}",
                true);

            return builder.Build();
        }

        public async Task<bool> HandleCallbackAsync(SocketReaction reaction)
        {
            var emote = reaction.Emote;

            if (emote.Equals(EmotesHelper.Emotes["pokeball"]))
            {
                if(_user.Bag.PokeBalls.Count(x => x is NormalBall) == 0)
                {
                    await _messageService.NewMessageAsync(Context, "You don't have have any Pokeballs left");
                    return false;
                }

                var ball = _user.Bag.PokeBalls.FirstOrDefault(x => x is NormalBall) as NormalBall;
                UseBall(ball);

                if(IsCaptured(ball))
                {
                    _ = CapturePokemonAsync();
                    return true;
                }

                await _messageService.NewMessageAsync(Context, $"Darn! The {_encounter.Name} escaped");
                await _message.ModifyAsync(x => x.Embed = BuildEmbed());
                _ = _message.RemoveReactionAsync(emote, Context.User);
            }

            return false;
        }

        private async Task CapturePokemonAsync()
        {
            caught = true;
            _ = _message.RemoveAllReactionsAsync();
            _player.UpdateDexEntry(_user, _encounter);
            await _messageService.NewMessageAsync(Context, $"{_encounter.Name} has been caught!");  
        }

        private void UseBall(BaseBall ball)
            => _player.UseBall(_user, ball);

        private bool IsCaptured(BaseBall ball)
        {
            if(ball is MasterBall) return true;
            var encRate = _encounter.CaptureRate;
            var ran = _random.Next(ball.CatchRate);
            return encRate > ran;
        }
    }
}
