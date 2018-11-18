using Discord;
using Discord.WebSocket;
using Espeon.Core.Attributes;
using Espeon.Core.Commands;
using Espeon.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Pusharp;
using Pusharp.Entities;
using Qmmands;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Espeon.Core;

namespace Espeon
{
    public class EspeonStartup
    {
        private readonly IServiceProvider _services;

        [Inject] private readonly DiscordSocketClient _client;
        [Inject] private readonly CommandService _commands;
        [Inject] private readonly PushBulletClient _push;

        private Device _phone;
        private Device Phone => _phone ?? (_phone = _push.Devices.First());

        private readonly TaskCompletionSource<Task> _completionSource;

        public EspeonStartup(IServiceProvider services)
        {
            _services = services;
            _completionSource = new TaskCompletionSource<Task>();
        }

        public async Task StartBotAsync()
        {
            EventHooks();

            var assembly = typeof(IEspeonContext).Assembly;
            await _commands.AddModulesAsync(assembly);

            await _push.ConnectAsync();

            await _client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("Testeon"));
            await _client.StartAsync();

            await _completionSource.Task;

            _services.RunInitialisers(Assembly.GetEntryAssembly());
        }

        private void EventHooks()
        {
            _client.Ready += () =>
            {
                _completionSource.SetResult(Task.CompletedTask);
                return Task.CompletedTask;
            };
            
            var logger = _services.GetService<ILogService>();
            _client.Log += async log =>
            {
                var (source, severity, lMessage, exception) = LogFactory.FromDiscord(log);
                await logger.LogAsync(source, severity, lMessage, exception);
            };

            _push.Log += async log =>
            {
                var (source, severity, lMessage) = LogFactory.FromPusharp(log);
                await logger.LogAsync(source, severity, lMessage);
            };

            var message = _services.GetService<IMessageService>();
            _client.MessageReceived += message.HandleReceivedMessageAsync;

            
#if !DEBUG //Don't want to waste my pushes
            _client.Connected += async () => 
            {
                await Phone.SendNoteAsync(x =>
                {
                    x.Title = "Espeon Connected";
                    x.Body = "<3";
                });

            };

            _client.Disconnected += async _ =>
            {
                await Phone.SendNoteAsync(x =>
                {
                    x.Title = "Espeon Disconnected";
                    x.Body = "</3";
                });
            };
#endif
        }
    }
}
