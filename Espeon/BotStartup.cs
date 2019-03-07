using Discord;
using Discord.WebSocket;
using Espeon.Databases.CommandStore;
using Espeon.Databases.Entities;
using Espeon.Databases.GuildStore;
using Espeon.Databases.UserStore;
using Espeon.Enums;
using Espeon.Extensions;
using Espeon.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Espeon
{
    public class BotStartup
    {
        private readonly IServiceProvider _services;

        [Inject] private readonly DiscordSocketClient _client;
        [Inject] private readonly CommandService _commands;

        private readonly Config _config;
        private bool _ran;

        public BotStartup(IServiceProvider services, Config config)
        {
            _services = services;
            _config = config;
            _ran = false;
        }

        public async Task StartAsync(UserStore userStore, CommandStore commandStore)
        {
            EventHooks(userStore);

            await SetupCommandsAsync(commandStore);

            await _client.LoginAsync(TokenType.Bot, _config.DiscordToken);
            await _client.StartAsync();
        }

        private async Task SetupCommandsAsync(CommandStore commandStore)
        {
            var dbModules = await commandStore.Modules.Include(x => x.Commands).ToArrayAsync();

            var modulesToCreate = new List<ModuleBuilder>();
            var commandsToCreate = new List<(ModuleBuilder Module, CommandBuilder Command)>();

            _commands.AddModules(Assembly.GetEntryAssembly(),
                action: moduleBuilder =>
                {
                    if (string.IsNullOrEmpty(moduleBuilder.Name))
                        throw new NoNullAllowedException(nameof(moduleBuilder));

                    var foundModule = dbModules.FirstOrDefault(x => x.Name == moduleBuilder.Name);

                    if (foundModule is null)
                    {
                        modulesToCreate.Add(moduleBuilder);
                        return;
                    }

                    if (!(foundModule.Aliases is null) && foundModule.Aliases.Count > 0)
                        moduleBuilder.AddAliases(foundModule.Aliases.ToArray());

                    var commandBuilders = moduleBuilder.Commands;

                    foreach (var commandBuilder in commandBuilders)
                    {
                        if (string.IsNullOrWhiteSpace(commandBuilder.Name))
                            throw new NoNullAllowedException(nameof(commandBuilder));

                        var foundCommand = foundModule.Commands.FirstOrDefault(x => x.Name == commandBuilder.Name);

                        if (foundCommand is null)
                        {
                            commandsToCreate.Add((moduleBuilder, commandBuilder));
                            continue;
                        }

                        if (!(foundCommand.Aliases is null) && foundCommand.Aliases.Count > 0)
                            commandBuilder.AddAliases(foundCommand.Aliases.ToArray());
                    }
                });

            foreach (var module in modulesToCreate)
            {
                var newModule = new ModuleInfo
                {
                    Name = module.Name
                };

                var commandBuilders = module.Commands;
                if (module.Commands.Any(x => string.IsNullOrEmpty(x.Name)))
                    throw new NoNullAllowedException(nameof(module));

                if (commandBuilders.Any(x => string.IsNullOrEmpty(x.Name)))
                    throw new NoNullAllowedException(nameof(commandBuilders));

                newModule.Commands = commandBuilders.Select(x => new CommandInfo
                {
                    Name = x.Name
                }).ToList();

                await commandStore.Modules.AddAsync(newModule);
            }

            foreach(var (module, command) in commandsToCreate)
            {
                var foundModule = await commandStore.Modules.FindAsync(module.Name);

                foundModule.Commands.Add(new CommandInfo
                {
                    Name = command.Name
                });
            }

            await commandStore.SaveChangesAsync();
        }

        //TODO clean this up
        private void EventHooks(UserStore userStore)
        {
            _client.Ready += async () =>
            {
                if (!_ran)
                {
                    await _services.GetService<ReminderService>().LoadRemindersAsync(userStore);
                    _ran = true;
                }
            };

            _client.UserJoined += async user =>
            {
                using var guildStore = _services.GetService<GuildStore>();

                var dbGuild = await guildStore.GetOrCreateGuildAsync(user.Guild);
                var guild = user.Guild;

                if (guild.GetTextChannel(dbGuild.WelcomeChannelId) is SocketTextChannel channel
                    && !string.IsNullOrWhiteSpace(dbGuild.WelcomeMessage))
                {
                    var str = dbGuild.WelcomeMessage
                        .Replace("{{guild}}", user.Guild.Name)
                        .Replace("{{user}}", user.GetDisplayName());

                    await channel.SendMessageAsync(user.Mention, embed: new EmbedBuilder
                    {
                        Title = "A User Appears!",
                        Color = Utilities.EspeonColor,
                        Description = str,
                        ThumbnailUrl = user.GetAvatarOrDefaultUrl()
                    }
                        .Build());
                }

                if (guild.GetRole(dbGuild.DefaultRoleId) is SocketRole role)
                {
                    await user.AddRoleAsync(role);
                }
            };

            _client.JoinedGuild += async guild =>
            {
                var channelName = new[] { "welcome", "introduction", "general" };

                var channel = guild.TextChannels
                    .FirstOrDefault(x => channelName.Any(y => x.Name.Contains(y, StringComparison.InvariantCultureIgnoreCase)))
                        ?? guild.TextChannels.FirstOrDefault(x => guild.CurrentUser.GetPermissions(x).ViewChannel
                            && guild.CurrentUser.GetPermissions(x).SendMessages);

                if (channel is null)
                    return;

                await channel.SendMessageAsync(string.Empty, embed: new EmbedBuilder
                {
                    Title = "",
                    Color = Utilities.EspeonColor,
                    ThumbnailUrl = guild.CurrentUser.GetAvatarOrDefaultUrl(),
                    Description = $"Hello! I am Espeon{_services.GetService<EmotesService>().Collection["Espeon"]} and I have just been added to your guild!\n" +
                    $"Type es/help to see all my available commands!"
                }
                    .Build());
            };

            var logger = _services.GetService<LogService>();
            _client.Log += log =>
            {
                return logger.LogAsync(Source.Discord, (Severity)(int)log.Severity, log.Message, log.Exception);
            };
        }
    }
}
