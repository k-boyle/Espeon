using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Umbreon.Core;
using Umbreon.Core.Models.Database;
using Umbreon.Preconditions;

namespace Umbreon.Services
{
    public class CustomCommandsService
    {
        private readonly DatabaseService _database;
        private readonly CommandService _commandService;
        private readonly MessageService _message;
        private readonly LogService _logs;

        public CustomCommandsService(DatabaseService database, CommandService commandsService, MessageService message, LogService logs)
        {
            _database = database;
            _commandService = commandsService;
            _message = message;
            _logs = logs;
        }

        public async Task LoadCmds(DiscordSocketClient client)
        {
            foreach (var guild in client.Guilds)
            {
                await NewCmds(guild.Id);
            }

            _logs.NewLogEvent(LogSeverity.Info, LogSource.CustomCmds, "Custom commands have been loaded");
        }

        private async Task NewCmds(ulong guildId)
        {
            var cmds = GetCmds(guildId);
            await _commandService.CreateModuleAsync(guildId.ToString(), module =>
            {
                module.WithName("Custom Commands");
                module.AddAliases("");
                module.AddPrecondition(new RequireGuild(guildId));

                foreach (var cmd in cmds)
                {
                    module.AddCommand(cmd.CommandName, CommandCallback, _ => { });
                }
            });
        }

        private async Task CreateNewCmd(ulong guildId)
        {
            await _commandService.RemoveModuleAsync(
                _commandService.Modules.FirstOrDefault(x =>
                    string.Equals(x.Name, guildId.ToString(), StringComparison.CurrentCultureIgnoreCase)));
            await NewCmds(guildId);
           _logs.NewLogEvent(LogSeverity.Info, LogSource.CustomCmds, $"New command has been created in {guildId}");
        }

        private async Task CommandCallback(ICommandContext context, object[] _, IServiceProvider __, CommandInfo info)
        {
            await _message.SendMessageAsync(context, GetCmds(context).FirstOrDefault(x =>
                string.Equals(x.CommandName, info.Name, StringComparison.CurrentCultureIgnoreCase)).CommandValue);
        }

        public async Task CreateCmd(ICommandContext context, string cmdName, string cmdValue)
        {
            var newCmd = new CustomCommand
            {
                CommandName = cmdName,
                CommandValue = cmdValue
            };
            var guild = _database.GetGuild(context);
            guild.CustomCommands.Add(newCmd);
            _database.UpdateGuild(guild);
            await CreateNewCmd(context.Guild.Id);
        }

        public void UpdateCommand(ICommandContext context, string cmdName, string newValue)
        {
            var guild = _database.GetGuild(context);
            guild.CustomCommands
                .Find(x => string.Equals(x.CommandName, cmdName, StringComparison.CurrentCultureIgnoreCase))
                .CommandValue = newValue;
            _database.UpdateGuild(guild);
        }

        public bool TryParse(IEnumerable<CustomCommand> cmds, string cmdName, out CustomCommand cmd)
        {
            cmd = cmds.FirstOrDefault(x => string.Equals(x.CommandName, cmdName, StringComparison.CurrentCultureIgnoreCase));
            return !(cmd is null);
        }

        public IEnumerable<CustomCommand> GetCmds(ICommandContext context)
            => GetCmds(context.Guild.Id);

        private IEnumerable<CustomCommand> GetCmds(ulong guildId)
        {
            return _database.GetGuild(guildId).CustomCommands;
        }
    }
}
