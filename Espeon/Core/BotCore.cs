using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Espeon.Attributes;
using Espeon.Core.Entities.Guild;
using Espeon.Extensions;
using Espeon.Helpers;
using Espeon.Services;

namespace Espeon.Core
{
    public class BotCore
    {
        private readonly IServiceProvider _services;
        private readonly IList<Type> _reflectedServices;
        private readonly TaskCompletionSource<Task> _completionSource;

        public BotCore(IServiceProvider services, IEnumerable<Type> reflectedServices)
        {
            _services = services;
            _reflectedServices = reflectedServices.ToList();
            _completionSource = new TaskCompletionSource<Task>();
        }

        public async Task RunBotAsync()
        {
            await DatabaseService.InitialiseAsync();
            _services.GetService<PokemonDataService>().Initialise();
            _services.GetService<PokemonPlayerService>().Initialise();

            HookEvents();
            await SetupCommandsAsync();

            var client = _services.GetService<DiscordSocketClient>();
            await client.LoginAsync(TokenType.Bot, ConstantsHelper.BotToken);
            await client.StartAsync();

            await _completionSource.Task;
            RunInitialisers();
            await Task.Delay(-1);
        }

        private void HookEvents()
        {
            var client = _services.GetService<DiscordSocketClient>();
            var logs = _services.GetService<LogService>();
            var message = _services.GetService<MessageService>();
            var commands = _services.GetService<CommandService>();
            client.Log += logs.LogEventAsync;
            client.Ready += () =>
            {
                _completionSource.SetResult(Task.CompletedTask);
                return Task.CompletedTask;
            };
            client.MessageReceived += message.HandleMessageAsync;
            client.MessageUpdated += (_, msg, __) => message.HandleMessageUpdateAsync(msg);
            client.JoinedGuild += async guild =>
            {
                var channel = guild.GetDefaultChannel();
                if (channel is null) return;

                await channel.SendMessageAsync(string.Empty, embed: new EmbedBuilder
                {
                    Author = new EmbedAuthorBuilder
                    {
                        IconUrl = client.CurrentUser.GetAvatarOrDefaultUrl(),
                        Name = guild.CurrentUser.GetDisplayName()
                    },
                    Color = new Color(0, 0, 0),
                    ThumbnailUrl = client.CurrentUser.GetDefaultAvatarUrl(),
                    Description =
                        $"Hello! I am Espeon{EmotesHelper.Emotes["Espeon"]} and I have just been added to your guild!\n" +
                        $"Type {(await _services.GetService<DatabaseService>().GetObjectAsync<GuildObject>("guilds", guild.Id)).Prefixes.First()}help to see all my available commands!"
                }.Build());
            };
            commands.Log += logs.LogEventAsync;
        }

        private async Task SetupCommandsAsync()
        {
            var commands = _services.GetService<CommandService>();

            var typereaders = AssemblyHelper.GetAllTypesWithAttribute<TypeReaderAttribute>();

            foreach (var typereader in typereaders)
            {
                var attribute = typereader.GetCustomAttribute<TypeReaderAttribute>();
                commands.AddTypeReader(attribute?.TargetType, (TypeReader)Activator.CreateInstance(typereader));
            }

            await commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        private void RunInitialisers()
        {
            foreach (var type in _reflectedServices)
            {
                var service = _services.GetService(type);

                foreach (var method in service.GetType().GetMethods())
                {
                    if (!(method.GetCustomAttribute<InitAttribute>() is InitAttribute attribute)) continue;
                    var argTypes = attribute.Arguments;
                    var args = argTypes.Select(x => _services.GetService(x)).ToArray();
                    method.Invoke(service, args);
                }
            }
        }
    }
}
