using Discord;
using Discord.WebSocket;
using Espeon.Core.Attributes;
using Espeon.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Espeon
{
    public class EspeonStartup
    {
        private readonly IServiceProvider _services;
        private readonly Assembly _assembly;

        [Inject] private readonly DiscordSocketClient _client;

        [Inject] private readonly CommandService _commands;

        public EspeonStartup(IServiceProvider services, Assembly assembly)
        {
            _services = services;
            _assembly = assembly;
        }

        public async Task StartBotAsync()
        {
            EventHooks();
            await _client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("Testeon"));
            await _client.StartAsync();

            await _commands.AddModulesAsync(_assembly);
        }

        private void EventHooks()
        {
            var logger = _services.GetService<ILogService>();
            _client.Log += logger.LogAsync;

            var message = _services.GetService<IMessageService>();
            _client.MessageReceived += message.HandleReceivedMessageAsync;
        }
    }
}
