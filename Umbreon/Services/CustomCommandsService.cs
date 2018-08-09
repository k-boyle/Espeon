using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Umbreon.Attributes;
using Umbreon.Core;
using Umbreon.Core.Models.Database.Guilds;
using Umbreon.Preconditions;

namespace Umbreon.Services
{
    [Service]
    public class CustomCommandsService
    {
        private readonly DatabaseService _database;
        private readonly CommandService _commandService;
        private readonly MessageService _message;
        private readonly LogService _logs;
        private readonly ConcurrentDictionary<ulong, ModuleInfo> _modules = new ConcurrentDictionary<ulong, ModuleInfo>();

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
            if (_modules.TryGetValue(guildId, out var found))
                await _commandService.RemoveModuleAsync(found);

            var cmds = GetCmds(guildId);
            var created = await _commandService.CreateModuleAsync(guildId.ToString(), module =>
            {
                module.WithName(guildId.ToString());
                module.AddAliases("");
                module.AddPrecondition(new RequireGuildAttribute(guildId));
                module.WithSummary("The custom commands for this server");

                foreach (var cmd in cmds)
                {
                    module.AddCommand(cmd.CommandName, CommandCallback, command =>
                    {
                        command.AddAttributes(new UsageAttribute($"{cmd.CommandName}"));
                        command.WithSummary("This is a custom command");
                        command.WithName(cmd.CommandName);
                    });
                }
            });

            _modules.TryAdd(guildId, created);
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
            await NewCmds(context.Guild.Id);
        }

        public void UpdateCommand(ICommandContext context, string cmdName, string newValue)
        {
            var guild = _database.GetGuild(context);
            guild.CustomCommands
                .Find(x => string.Equals(x.CommandName, cmdName, StringComparison.CurrentCultureIgnoreCase))
                .CommandValue = newValue;
            _database.UpdateGuild(guild);
        }

        public async Task RemoveCmd(ICommandContext context, string cmdName)
        {
            var guild = _database.GetGuild(context);
            var targetCmd = guild.CustomCommands.Find(x =>
                string.Equals(x.CommandName, cmdName, StringComparison.CurrentCultureIgnoreCase));
            guild.CustomCommands.Remove(targetCmd);
            _database.UpdateGuild(guild);
            await NewCmds(context.Guild.Id);
        }

        public bool IsReserved(string toCheck)
        {
            var cmds = _commandService.Commands.SelectMany(x => x.Aliases).ToList();
            var reserved = new List<string>();
            foreach (var cmd in cmds)
            {
                reserved.Add(RemoveGroupName(cmd));
            }

            return reserved.Contains(toCheck, StringComparer.CurrentCultureIgnoreCase);
        }

        private string RemoveGroupName(string inStr)
        {
            var groups = _commandService.Modules.Where(x => !(x.Group is null)).Select(y => y.Group);
            var outStr = inStr;
            foreach (var group in groups)
            {
                if (inStr.Contains(group))
                {
                    outStr = inStr.Replace($"{group} ", "");
                }
            }
            return outStr;
        }

        public bool TryParse(IEnumerable<CustomCommand> cmds, string cmdName, out CustomCommand cmd)
        {
            cmd = cmds.FirstOrDefault(x => string.Equals(x.CommandName, cmdName, StringComparison.CurrentCultureIgnoreCase));
            return !(cmd is null);
        }

        public IEnumerable<CustomCommand> GetCmds(ICommandContext context)
            => GetCmds(context.Guild.Id);

        private IEnumerable<CustomCommand> GetCmds(ulong guildId)
            => _database.GetGuild(guildId).CustomCommands;
    }
}
