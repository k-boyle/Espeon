using Casino.DependencyInjection;
using Casino.Discord;
using Discord;
using Discord.WebSocket;
using Espeon.Commands;
using Espeon.Databases;
using Espeon.Databases.CommandStore;
using Espeon.Databases.GuildStore;
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
using Module = Qmmands.Module;

namespace Espeon.Bot.Services
{
    public class CommandHandlingService : BaseService<InitialiseArgs>, ICommandHandlingService
    {
        [Inject] private readonly DiscordSocketClient _client;
        [Inject] private readonly CommandService _commands;
        [Inject] private readonly ICustomCommandsService _customCommands;
        [Inject] private readonly IEventsService _events;
        [Inject] private readonly ILogService _logger;
        [Inject] private readonly IMessageService _message;
        [Inject] private readonly IServiceProvider _services;

        private string[] _botMentions;

        public CommandHandlingService(IServiceProvider services) : base(services)
        {
            _client.MessageReceived += async msg =>
            {
                if (msg is SocketUserMessage message)
                    await _events.RegisterEvent(() => HandleMessageAsync(message));
            };

            _client.MessageUpdated += async (cache, after, _) =>
            {
                var before = await cache.GetOrDownloadAsync();

                if (before.Content == after.Content)
                    return;

                if (after is SocketUserMessage message)
                    await _events.RegisterEvent(() => HandleMessageAsync(message));
            };

            _commands.CommandExecutionFailed += args => _events.RegisterEvent(() => CommandExecutionFailedAsync(args));
            _commands.CommandExecuted += args => _events.RegisterEvent(() => CommandExecutedAsync(args));
        }

        async Task ICommandHandlingService.SetupCommandsAsync(CommandStore commandStore)
        {
            var dbModules = await commandStore.Modules.Include(x => x.Commands).ToArrayAsync();

            var modulesToCreate = new List<ModuleBuilder>();
            var commandsToCreate = new List<CommandBuilder>();

            var modules = _commands.AddModules(Assembly.GetEntryAssembly(),
                action: moduleBuilder =>
                {
                    if (string.IsNullOrEmpty(moduleBuilder.Name))
                        throw new NoNullAllowedException(nameof(moduleBuilder));

                    var foundModule = Array.Find(dbModules, x => x.Name == moduleBuilder.Name);

                    if (foundModule is null)
                    {
                        modulesToCreate.Add(moduleBuilder);
                        return;
                    }

                    if (foundModule.Aliases?.Count > 0)
                        moduleBuilder.AddAliases(foundModule.Aliases);

                    foreach (var commandBuilder in moduleBuilder.Commands)
                    {
                        if (string.IsNullOrWhiteSpace(commandBuilder.Name))
                            throw new NoNullAllowedException(nameof(commandBuilder));

                        var foundCommand = foundModule.Commands.Find(x => x.Name == commandBuilder.Name);

                        if (foundCommand is null)
                        {
                            commandsToCreate.Add(commandBuilder);
                            continue;
                        }

                        if (foundCommand.Aliases?.Count > 0)
                            commandBuilder.AddAliases(foundCommand.Aliases);
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

            foreach (var command in commandsToCreate)
            {
                var foundModule = await commandStore.Modules.FindAsync(command.Module.Name);

                foundModule.Commands.Add(new CommandInfo
                {
                    Name = command.Name
                });

                commandStore.Update(foundModule);
            }

            await commandStore.SaveChangesAsync();

            _services.GetService<IResponseService>()?.LoadResponses(modules);
        }

        private async Task HandleMessageAsync(SocketUserMessage message)
        {
            await HandleMessageAsync(message.Author, message.Channel, message.Content, message);
        }

        private async Task HandleMessageAsync(SocketUser author, ISocketMessageChannel channel,
            string content, SocketUserMessage message)
        {
            if(_botMentions is null)
                _botMentions = new[] { $"<@!{_client.CurrentUser.Id}> ", $"<@{_client.CurrentUser.Id}> " };

            if (author.IsBot && author.Id != _client.CurrentUser.Id)
                return;

            if (!(channel is SocketTextChannel textChannel)
                || !textChannel.Guild.CurrentUser.GetPermissions(textChannel).Has(ChannelPermission.SendMessages))
            {
                return;
            }

            IReadOnlyCollection<string> prefixes;

            using (var guildStore = _services.GetService<GuildStore>())
            {
                var guild = await guildStore.GetOrCreateGuildAsync(textChannel.Guild);
                prefixes = guild.Prefixes;

                if (guild.AutoQuotes)
                {
                    _ = Task.Run(async () =>
                    {
                        var embed = await Utilities.QuoteFromStringAsync(_client, content);

                        if (embed is null)
                            return;

                        await channel.SendMessageAsync(string.Empty, embed: embed);
                    });
                }

                if (guild.RestrictedChannels.Contains(textChannel.Id) || guild.RestrictedUsers.Contains(author.Id))
                    return;
            }


            if (CommandUtilities.HasAnyPrefix(content, prefixes, StringComparison.CurrentCulture,
                    out var prefix, out var output)
                || CommandUtilities.HasAnyPrefix(content, _botMentions, out prefix, out output))
            {
                if (string.IsNullOrWhiteSpace(output))
                    return;

                try
                {
                    var commandContext = await EspeonContext.CreateAsync(_client, message, prefix);

                    var result = await _commands.ExecuteAsync(output, commandContext, _services);

                    bool CheckForCustom(Module module)
                    {
                        return result is ChecksFailedResult
                            && ulong.TryParse(module.Name, out var id)
                            && _customCommands.IsCustomCommand(id);
                    }

                    if (result is CommandNotFoundResult || CheckForCustom(commandContext.Command.Module))
                    {
                        commandContext = await EspeonContext.CreateAsync(_client, message, prefix);
                        result = await _commands.ExecuteAsync($"help {output}", commandContext, _services);
                    }

                    if (!result.IsSuccessful && !(result is ExecutionFailedResult))
                    {
                        await CommandExecutionFailedAsync(new EspeonCommandErroredEventArgs
                        {
                            Context = commandContext,
                            Result = result as FailedResult
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger.Log(Source.Commands, Severity.Error, string.Empty, ex);
                }
            }
        }

        async Task ICommandHandlingService.ExecuteCommandAsync(SocketUser author, ISocketMessageChannel channel,
            string content, SocketUserMessage message)
        {
            await HandleMessageAsync(author, channel, content, message);
        }

        private async Task CommandExecutionFailedAsync(EspeonCommandErroredEventArgs args)
        {
            var context = args.Context;

            if (args.Result is ExecutionFailedResult failed)
            {
                _logger.Log(Source.Commands, Severity.Error, string.Empty, failed.Exception);

#if !DEBUG
                var c = _client.GetChannel(463299724326469634) as SocketTextChannel;

                var ex = failed.Exception.ToString();

                await c.SendMessageAsync(Format.Sanitize(ex.Length > 1000 ? ex.Substring(0, 1000) : ex));
#endif
            }

            await _message.SendAsync(context, x => x.Embed = Utilities.BuildErrorEmbed(args.Result, context));
        }

        private Task CommandExecutedAsync(CommandExecutedEventArgs args)
        {
            var context = (EspeonContext)args.Context;

            _logger.Log(Source.Commands, Severity.Verbose,
                $"Successfully executed {{{context.Command.Name}}} for " +
                $"{{{context.User.GetDisplayName()}}} in {{{context.Guild.Name}/{context.Channel.Name}}}");

            return Task.CompletedTask;
        }

        private struct EspeonCommandErroredEventArgs
        {
            public EspeonContext Context { get; set; }
            public FailedResult Result { get; set; }

            public static implicit operator EspeonCommandErroredEventArgs(CommandExecutionFailedEventArgs args)
                => new EspeonCommandErroredEventArgs
                {
                    Context = (EspeonContext)args.Context,
                    Result = args.Result
                };
        }
    }
}
