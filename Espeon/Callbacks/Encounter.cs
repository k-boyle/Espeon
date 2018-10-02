using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Espeon.Core.Entities.Pokemon;
using Espeon.Core.Entities.Pokemon.Pokeballs;
using Espeon.Core.Entities.User;
using Espeon.Extensions;
using Espeon.Helpers;
using Espeon.Interactive;
using Espeon.Interactive.Callbacks;
using Espeon.Interactive.Criteria;
using Espeon.Interactive.Paginator;
using Espeon.Services;

namespace Espeon.Callbacks
{
    public class Encounter : IReactionCallback
    {
        public RunMode RunMode => RunMode.Sync;
        public ICriterion<SocketReaction> Criterion => new EnsureReactionFromSourceUserCriterion();
        public TimeSpan? Timeout => TimeSpan.FromMinutes(2);
        public ICommandContext Context { get; }

        private readonly PokemonData _encounter;
        private readonly ulong _playerId;
        private Task<UserObject> GetUserAsync() => _player.GetCurrentPlayerAsync(_playerId);
        private async Task<int> GetPokeBallsAsync() => (await GetUserAsync()).Bag.PokeBalls.Count(x => x is NormalBall);
        private async Task<int> GetGreatballsAsync() => (await GetUserAsync()).Bag.PokeBalls.Count(x => x is GreatBall);
        private async Task<int> GetUltraballsAsync() => (await GetUserAsync()).Bag.PokeBalls.Count(x => x is UltraBall);
        private async Task<int> GetMasterballsAsync() => (await GetUserAsync()).Bag.PokeBalls.Count(x => x is MasterBall);

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
                embed: await BuildEmbedAsync());

            _ = Task.Run(async () =>
            {
                if(await GetPokeBallsAsync() > 0)
                    await _message.AddReactionAsync(EmotesHelper.Emotes["pokeball"], new RequestOptions
                    {
                        BypassBuckets = true
                    });

                if(await GetGreatballsAsync() > 0)
                    await _message.AddReactionAsync(EmotesHelper.Emotes["greatball"], new RequestOptions
                    {
                        BypassBuckets = true
                    });

                if(await GetUltraballsAsync() > 0)
                    await _message.AddReactionAsync(EmotesHelper.Emotes["ultraball"], new RequestOptions
                    {
                        BypassBuckets = true
                    });

                if (await GetMasterballsAsync() > 0)
                    await _message.AddReactionAsync(EmotesHelper.Emotes["masterball"], new RequestOptions
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
                    await _message.ModifyAsync(async x => x.Embed = await BuildEmbedAsync());
                }
                _player.SetEncounter(Context.User.Id, false);
            });

        }

        private async Task<Embed> BuildEmbedAsync()
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
                $"{EmotesHelper.Emotes["pokeball"]} Pokeballs: {await GetPokeBallsAsync()}\n" +
                $"{EmotesHelper.Emotes["greatball"]} Greatballs: {await GetGreatballsAsync()}\n" +
                $"{EmotesHelper.Emotes["ultraball"]} Ultraballs: {await GetUltraballsAsync()}\n" +
                $"{EmotesHelper.Emotes["masterball"]} Masterballs: {await GetMasterballsAsync()}",
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
                if(await GetPokeBallsAsync() == 0)
                {
                    _battleLog.Add("You are out of Pokeballs");
                    return false;
                }

                ball = (await GetUserAsync()).Bag.PokeBalls.OfType<NormalBall>().FirstOrDefault();
            }

            if (emote.Equals(EmotesHelper.Emotes["greatball"]))
            {
                if (await GetGreatballsAsync() == 0)
                {
                    _battleLog.Add("You are out of Great balls");
                    return false;
                }

                ball = (await GetUserAsync()).Bag.PokeBalls.OfType<GreatBall>().FirstOrDefault();
            }

            if (emote.Equals(EmotesHelper.Emotes["ultraball"]))
            {
                if (await GetUltraballsAsync() == 0)
                {
                    _battleLog.Add("You are out of Ultra balls");
                    return false;
                }

                ball = (await GetUserAsync()).Bag.PokeBalls.OfType<UltraBall>().FirstOrDefault();
            }

            if (emote.Equals(EmotesHelper.Emotes["masterball"]))
            {
                if (await GetMasterballsAsync() == 0)
                {
                    _battleLog.Add("You are out of Master balls");
                    return false;
                }

                ball = (await GetUserAsync()).Bag.PokeBalls.OfType<MasterBall>().FirstOrDefault();
            }

            if (emote.Equals(new Emoji("❌")))
            {
                await EndEncounterAsync("You have fled");
                return true;
            }

            await UseBallAsync(ball);
            _attemps++;

            if (IsCaptured(ball))
            {
                _ = CapturePokemonAsync();
                return true;
            }

            if (_attemps == _fleeCount)
            {
                await EndEncounterAsync($"The wild {_encounter.Name.FirstLetterToUpper()} has fled!");
                return true;
            }

            if (await GetPokeBallsAsync() == await GetGreatballsAsync() && await GetGreatballsAsync() == await GetUltraballsAsync() && await GetUltraballsAsync() == await GetMasterballsAsync() && await GetMasterballsAsync() == 0)
            {
                await EndEncounterAsync("You are out of balls");
                return true;
            }

            _battleLog.Add($"{_encounter.Name.FirstLetterToUpper()} escaped the ball!");
            await _message.ModifyAsync(async x => x.Embed = await BuildEmbedAsync());
            _ = _message.RemoveReactionAsync(emote, Context.User);

            return false;
        }

        private async Task EndEncounterAsync(string logMessage)
        {
            _caught = true;
            _player.SetEncounter(Context.User.Id, false);
            _ = _message.RemoveAllReactionsAsync();
            _battleLog.Add(logMessage);
            await _message.ModifyAsync(async x => x.Embed = await BuildEmbedAsync());
        }

        private async Task CapturePokemonAsync()
        {
            _player.UpdateDexEntry(await GetUserAsync(), _encounter);
            await EndEncounterAsync($"{_encounter.Name.FirstLetterToUpper()} has been captured!");
        }

        private async Task UseBallAsync(BaseBall ball)
            => _player.UseBall(await GetUserAsync(), ball);

        private bool IsCaptured(BaseBall ball)
        {
            if(ball is MasterBall) return true;
            var encRate = _encounter.CaptureRate;
            var ran = _random.Next(ball.CatchRate);
            return encRate > ran;
        }
    }
}
