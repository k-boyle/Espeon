using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        private UserObject User => _player.GetCurrentPlayer(_playerId);
        private int Pokeballs => User.Bag.PokeBalls.Count(x => x is NormalBall);
        private int Greatballs => User.Bag.PokeBalls.Count(x => x is GreatBall);
        private int Ultraballs => User.Bag.PokeBalls.Count(x => x is UltraBall);

        private readonly InteractiveService _interactive;
        private readonly MessageService _messageService;
        private readonly PokemonPlayerService _player;
        private readonly PokemonDataService _data;
        private IUserMessage _message;
        private readonly Random _random;

        private bool _caught;
        private int _attemps;
        private readonly int _fleeCount;

        private readonly List<string> _battleLog = new List<string>();

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

            _player.SetEncounter(Context.User.Id, true);
            _fleeCount = _random.Next(1, 5);
        }

        public async Task SetupAsync()
        {
            _message = await _messageService.SendFileAsync(Context, PokemonDataService.GetImage(_encounter),
                embed: BuildEmbed());

            _ = Task.Run(async () =>
            {
                if(Pokeballs > 0)
                    await _message.AddReactionAsync(EmotesHelper.Emotes["pokeball"], new RequestOptions
                    {
                        BypassBuckets = true
                    });

                if(Greatballs > 0)
                    await _message.AddReactionAsync(EmotesHelper.Emotes["greatball"], new RequestOptions
                    {
                        BypassBuckets = true
                    });

                if(Ultraballs > 0)
                    await _message.AddReactionAsync(EmotesHelper.Emotes["ultraball"], new RequestOptions
                    {
                        BypassBuckets = true
                    });

                await _message.AddReactionAsync(new Emoji("❌"), new RequestOptions
                {
                    BypassBuckets = true
                });
            });

            _interactive.AddReactionCallback(_message, this);

            _ = Task.Delay(Timeout.GetValueOrDefault()).ContinueWith(async _ =>
            {
                _interactive.RemoveReactionCallback(_message);
                _ = _message.RemoveAllReactionsAsync();
                if (!_caught)
                {
                    _battleLog.Add($"The wild {_encounter.Name.FirstLetterToUpper()} has fled!");
                    await _message.ModifyAsync(x => x.Embed = BuildEmbed());
                }
                _player.SetEncounter(Context.User.Id, false);
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
            
            builder.AddField($"A {_encounter.Name.FirstLetterToUpper()} appeared!", 
                $"Catch rate: {_encounter.CaptureRate}\n" +
                $"Encounter rate: {_encounter.EncounterRate}", 
                true);

            builder.AddField("Bag", 
                $"{EmotesHelper.Emotes["pokeball"]} Pokeballs: {Pokeballs}\n" +
                $"{EmotesHelper.Emotes["greatball"]} Greatballs: {Greatballs}\n" +
                $"{EmotesHelper.Emotes["ultraball"]} Ultraballs: {Ultraballs}",
                true);

            builder.AddField("Capture Log", _battleLog.Count > 0 ? string.Join('\n', _battleLog) : "\u200b");

            return builder.Build();
        }

        public async Task<bool> HandleCallbackAsync(SocketReaction reaction)
        {
            var emote = reaction.Emote;


            BaseBall ball = null;

            if (emote.Equals(EmotesHelper.Emotes["pokeball"]))
            {
                if(Pokeballs == 0)
                {
                    _battleLog.Add("You are out of Pokeballs");
                    return false;
                }

                ball = User.Bag.PokeBalls.FirstOrDefault(x => x is NormalBall) as NormalBall;
            }

            if (emote.Equals(EmotesHelper.Emotes["greatball"]))
            {
                if (Greatballs == 0)
                {
                    _battleLog.Add("You are out of Great balls");
                    return false;
                }

                ball = User.Bag.PokeBalls.FirstOrDefault(x => x is GreatBall) as GreatBall;
            }

            if (emote.Equals(EmotesHelper.Emotes["ultraball"]))
            {
                if (Pokeballs == 0)
                {
                    _battleLog.Add("You are out of Ultra balls");
                    return false;
                }

                ball = User.Bag.PokeBalls.FirstOrDefault(x => x is UltraBall) as UltraBall;
            }

            if (emote.Equals(new Emoji("❌")))
            {
                await EndEncounter("You have fled");
                return true;
            }

            UseBall(ball);
            _attemps++;

            if (IsCaptured(ball))
            {
                _ = CapturePokemonAsync();
                return true;
            }

            if (_attemps == _fleeCount)
            {
                await EndEncounter($"The wild {_encounter.Name.FirstLetterToUpper()} has fled!");
                return true;
            }

            if (Pokeballs == Greatballs && Greatballs == Ultraballs && Ultraballs == 0)
            {
                await EndEncounter("You are out of balls");
                return true;
            }

            _battleLog.Add($"{_encounter.Name.FirstLetterToUpper()} escaped the ball!");
            await _message.ModifyAsync(x => x.Embed = BuildEmbed());
            _ = _message.RemoveReactionAsync(emote, Context.User);

            return false;
        }

        private async Task EndEncounter(string logMessage)
        {
            _caught = true;
            _player.SetEncounter(Context.User.Id, false);
            _ = _message.RemoveAllReactionsAsync();
            _battleLog.Add(logMessage);
            await _message.ModifyAsync(x => x.Embed = BuildEmbed());
        }

        private async Task CapturePokemonAsync()
        {
            _player.UpdateDexEntry(User, _encounter);
            await EndEncounter($"{_encounter.Name.FirstLetterToUpper()} has been captured!");
        }

        private void UseBall(BaseBall ball)
            => _player.UseBall(User, ball);

        private bool IsCaptured(BaseBall ball)
        {
            if(ball is MasterBall) return true;
            var encRate = _encounter.CaptureRate;
            var ran = _random.Next(ball.CatchRate);
            return encRate > ran;
        }
    }
}
