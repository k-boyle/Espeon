using Discord;
using Discord.WebSocket;
using Espeon.Core.Attributes;
using Espeon.Core.Commands;
using Espeon.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Pusharp;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Espeon
{
    public class EspeonStartup
    {
        private readonly IServiceProvider _services;

        [Inject] private readonly DiscordSocketClient _client;
        [Inject] private readonly CommandService _commands;
        [Inject] private readonly PushBulletClient _push;

        public EspeonStartup(IServiceProvider services)
        {
            _services = services;
        }

        public async Task StartBotAsync()
        {
            EventHooks();
            await _client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("Testeon"));
            await _client.StartAsync();

            var assembly = typeof(IEspeonContext).Assembly;
            await _commands.AddModulesAsync(assembly);

            await _push.ConnectAsync();
        }

        private void EventHooks()
        {
            var module = _services.GetService<IModuleManager>();
            _commands.ModuleBuilding += module.OnBuildingAsync;

            var logger = _services.GetService<ILogService>();
            //_client.Log += logger.LogAsync;

            var message = _services.GetService<IMessageService>();
            _client.MessageReceived += message.HandleReceivedMessageAsync;
        }
    }
}
