using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;
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

        public async Task HandleMessageAsync(SocketMessage msg)
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
                            await context.Textchannel.SendMessageAsync(result.ErrorReason);
                    }
                }
            }
        }
    }
}
