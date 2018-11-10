using Discord;
using Discord.WebSocket;
using Espeon.Core.Attributes;
using Espeon.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon
{
    public class EspeonStartup
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

            var assembly = AppDomain.CurrentDomain.GetAssemblies().Single(x => x.GetName().Name == "Espeon.Core");
            await _commands.AddModulesAsync(assembly);
        }

        private void EventHooks()
        {
            var module = _services.GetService<IModuleManager>();
            _commands.ModuleBuilding += module.OnBuildingAsync;

            var logger = _services.GetService<ILogService>();
            _client.Log += logger.LogAsync;

            var message = _services.GetService<IMessageService>();
            _client.MessageReceived += message.HandleReceivedMessageAsync;
        }
    }
}
