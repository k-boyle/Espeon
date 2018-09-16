using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Umbreon.Attributes;
using Umbreon.Commands.TypeReaders;
using Umbreon.Core.Entities.Guild;
using Umbreon.Core.Entities.Pokemon;
using Umbreon.Extensions;
using Umbreon.Helpers;
using Umbreon.Services;

namespace Umbreon.Core
{
    public class BotCore
    {
        private readonly IServiceProvider _services;

        public BotCore(IServiceProvider services)
        {
            _services = services;
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
            
            await Task.Delay(-1);
        }

        private void HookEvents()
        {
            var client = _services.GetService<DiscordSocketClient>();
            var logs = _services.GetService<LogService>();
            var message = _services.GetService<MessageService>();
            var commands = _services.GetService<CommandService>();
            client.Log += logs.LogEvent;
            client.Ready += async () =>
            {
                await _services.GetService<CustomCommandsService>().LoadCmdsAsync(client);
                //await _musicService.InitialiseAsync();
                await _services.GetService<RemindersService>().LoadRemindersAsync();
            };
            client.MessageReceived += message.HandleMessageAsync;
            client.MessageUpdated += (_, msg, __) => message.HandleMessageUpdateAsync(msg);
            client.JoinedGuild += async guild =>
            {
                var channel = guild.GetDefaultChannel();
                if (!(channel is null))
                {
                    await channel.SendMessageAsync(string.Empty, embed: new EmbedBuilder
                    {
                        Author = new EmbedAuthorBuilder
                        {
                            IconUrl = client.CurrentUser.GetAvatarOrDefaultUrl(),
                            Name = guild.CurrentUser.GetDisplayName()
                        },
                        Color = new Color(0, 0, 0),
                        ThumbnailUrl = client.CurrentUser.GetDefaultAvatarUrl(),
                        Description = $"Hello! I am Umbreon{EmotesHelper.Emotes["umbreon"]} and I have just been added to your guild!\n" +
                                      $"Type {(await _services.GetService<DatabaseService>().GetObjectAsync<GuildObject>("guilds", guild.Id)).Prefixes.First()}help to see all my available commands!"
                    }.Build());
                }
            };
            commands.Log += logs.LogEvent;
        }

        private async Task SetupCommandsAsync()
        {
            var commands = _services.GetService<CommandService>();

            var typereaders = AssemblyHelper.GetAllTypesWithAttribute<TypeReaderAttribute>();

            foreach (var typereader in typereaders)
            {
                var attribute = typereader.GetCustomAttributes().OfType<TypeReaderAttribute>().FirstOrDefault();
                commands.AddTypeReader(attribute?.TargetType, (TypeReader)Activator.CreateInstance(typereader));
            }
            
            await commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }
    }
}
