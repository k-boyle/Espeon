using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;
using Umbreon.Attributes;
using Umbreon.Helpers;
using Umbreon.Modules.Contexts;

namespace Umbreon.Services
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly DatabaseService _database;
        private readonly MessageService _message;
        private readonly IServiceProvider _services;

        public CommandHandler(DiscordSocketClient client, CommandService commands, DatabaseService database, MessageService message, IServiceProvider services)
        {
            _client = client;
            _commands = commands;
            _database = database;
            _message = message;
            _services = services;
        }

        public async Task HandleMessageAsync(SocketMessage msg) // TODO custom command alias'
        {
            if (msg is SocketUserMessage message)
            {
                if (message.Channel is IDMChannel || message.Author.IsBot || message.Author.IsWebhook) return;
                {
                    var context = new GuildCommandContext(_client, message);
                    if (!context.Guild.CurrentUser.GetPermissions(context.Channel).SendMessages) return;
                    _message.SetCurrentMessage(message.Id);
                    var guild = _database.GetGuild(context);
                    var argPos = 0;
                    if (guild.Prefixes.Any(x => message.HasStringPrefix(x, ref argPos)) || message.HasMentionPrefix(_client.CurrentUser, ref argPos))
                    {
                        var result = await _commands.ExecuteAsync(context, argPos, _services);
                        if (!result.IsSuccess)
                        {
                            switch (result.Error)
                            {
                                case CommandError.UnknownCommand:
                                    if (guild.CloseCommandMatching)
                                    {
                                        var closest = _commands.Commands.FirstOrDefault(x => StringHelper.CalcLevenshteinDistance(x.Aliases.FirstOrDefault(), message.Content.Substring(argPos)) < 2);
                                        if (closest is null)
                                        {
                                            await _message.SendMessageAsync(context,
                                                "No command or close matching command found");
                                            break;
                                        }

                                        await _commands.ExecuteAsync(context, closest.Aliases.FirstOrDefault(),
                                            _services);
                                        break;
                                    }

                                    var commands = _commands.Commands.Where(x => StringHelper.CalcLevenshteinDistance(x.Aliases.FirstOrDefault(), message.Content.Substring(argPos)) < 5).Select(x => x.Aliases.FirstOrDefault()).Distinct();
                                    await _message.SendMessageAsync(context, $"{(commands.Any() ? "Command not found. Did you mean one of these?\n" + $"{string.Join("\n", commands)}" : "No commands found")}");
                                    break;
                                case CommandError.BadArgCount:
                                    var foundCommand = _commands.Search(context, argPos).Commands.FirstOrDefault().Command;
                                    await _message.SendMessageAsync(context, "Wrong command usage, have an example:\n" +
                                                                             $"{guild.Prefixes.First()}{(foundCommand.Attributes.FirstOrDefault(x => x is Usage) as Usage).Example}\n\n" +
                                                                             $"If you need more help with the command, simply type {guild.Prefixes.First()}help {foundCommand.Name}");
                                    break;

                                case CommandError.UnmetPrecondition:
                                    await _message.SendMessageAsync(context, $"There was an unmet precondition: {result.ErrorReason}");
                                    break;

                                case CommandError.Exception:
                                    await (_client.GetChannel(463299724326469634) as SocketTextChannel)
                                        .SendMessageAsync($"{message} : {result.ErrorReason}");
                                    break;

                                default:
                                    await _message.SendMessageAsync(context, result.ErrorReason);
                                    break;
                            }
                        }
                    }
                }
            }
        }
    }
}
