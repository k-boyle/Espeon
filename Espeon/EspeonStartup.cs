using Discord;
using Discord.WebSocket;
using Espeon.Core.Attributes;
using Espeon.Core.Commands;
using Espeon.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Espeon
{
    public class EspeonStartup<T> where T : IEspeonContext
    {
        private readonly IServiceProvider _services;

        [Inject] private readonly DiscordSocketClient _client;

        [Inject] private readonly CommandService _commands;

        public EspeonStartup(IServiceProvider services)
        {
            _services = services;
        }

        public async Task StartBotAsync()
        {
            EventHooks();
            await _client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("Testeon"));
            await _client.StartAsync();

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        private void EventHooks()
        {
            var logger = _services.GetService<ILogService>();
            _client.Log += logger.LogAsync;

            var message = _services.GetService<IMessageService<T>>();
            _client.MessageReceived += message.HandleReceivedMessageAsync;
        }
    }
}
