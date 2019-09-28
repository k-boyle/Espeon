using Casino.DependencyInjection;
using Espeon.Bot.Commands;
using Espeon.Commands;
using Espeon.Databases;
using Espeon.Services;
using Qmmands;
using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Bot.Services
{
    public class CustomCommandsService : BaseService<InitialiseArgs>, ICustomCommandsService
    {
        [Inject] private readonly ILogService _log;
        [Inject] private readonly IMessageService _message;
        [Inject] private readonly CommandService _commands;

        private readonly ConcurrentDictionary<ulong, Module> _moduleCache;

        public CustomCommandsService(IServiceProvider services) : base(services)
        {
            _moduleCache = new ConcurrentDictionary<ulong, Module>();
        }

        public override async Task InitialiseAsync(IServiceProvider services, InitialiseArgs args)
        {
            var guilds = await args.GuildStore.GetAllGuildsAsync(x => x.Commands);

            var createCommands = guilds.Select(CreateCommandsAsync);

            await Task.WhenAll(createCommands);
            _log.Log(Source.Commands, Severity.Verbose, "All custom commands loaded");
        }

        private Task CreateCommandsAsync(Guild guild)
        {
            var commands = guild.Commands;

            if (commands is null || commands.Count == 0)
                return Task.CompletedTask;

            _moduleCache[guild.Id] = _commands.AddModule(moduleBuilder =>
            {
                moduleBuilder.Name = guild.Id.ToString();
                moduleBuilder.AddCheck(new RequireGuildAttribute(guild.Id));

                foreach (var command in commands)
                {
                    if (command.Name is null)
                        continue;

                    moduleBuilder.AddCommand(CommandCallbackAsync, commandBuilder =>
                    {
                        commandBuilder.Name = command.Name;
                        commandBuilder.AddAlias(command.Name);
                    });
                }
            });

            return Task.CompletedTask;
        }

        private async ValueTask<CommandResult> CommandCallbackAsync(CommandContext originalContext, IServiceProvider services)
        {
            var context = (EspeonContext)originalContext;

            var guild = await context.GuildStore.GetOrCreateGuildAsync(context.Guild, x => x.Commands);
            var commands = guild?.Commands;

            var found = commands?.FirstOrDefault(x =>
                string.Equals(x.Name, context.Command.Name, StringComparison.InvariantCultureIgnoreCase));

            await _message.SendAsync(context, x => x.Content = found.Value);

            return null;
        }

        async Task<bool> ICustomCommandsService.TryCreateCommandAsync(EspeonContext context, string name, string value)
        {
            if (_moduleCache.TryGetValue(context.Guild.Id, out var found))
            {
                var commands = found.Commands;

                if (!Utilities.AvailableName(commands, name))
                    return false;
            }

            var loadedCommands = _commands.GetAllCommands();
            var filtered = loadedCommands.Where(x => !ulong.TryParse(x.Module.Name, out _));

            if (!Utilities.AvailableName(filtered, name))
                return false;

            var newCmd = new CustomCommand
            {
                Name = name,
                Value = value
            };

            var guild = await context.GuildStore.GetOrCreateGuildAsync(context.Guild, x => x.Commands);
            guild.Commands.Add(newCmd);
            context.GuildStore.Update(guild);

            await context.GuildStore.SaveChangesAsync();
            await UpdateCommandsAsync(guild);

            return true;
        }

        async Task ICustomCommandsService.DeleteCommandAsync(EspeonContext context, CustomCommand command)
        {
            var guild = await context.GuildStore.GetOrCreateGuildAsync(context.Guild, x => x.Commands);
            guild.Commands.Remove(command);
            context.GuildStore.Update(guild);

            await context.GuildStore.SaveChangesAsync();

            await UpdateCommandsAsync(guild);
        }

        Task ICustomCommandsService.ModifyCommandAsync(EspeonContext context, CustomCommand command, string newValue)
        {
            command.Value = newValue;
            context.GuildStore.Update(command);

            return context.GuildStore.SaveChangesAsync();
        }

        async Task<ImmutableArray<CustomCommand>> ICustomCommandsService.GetCommandsAsync(EspeonContext context)
        {
            var guild = await context.GuildStore.GetOrCreateGuildAsync(context.Guild, x => x.Commands);
            return guild.Commands.ToImmutableArray();
        }

        bool ICustomCommandsService.IsCustomCommand(ulong id)
            => _moduleCache.TryGetValue(id, out _);

        private async Task UpdateCommandsAsync(Guild guild)
        {
            if(_moduleCache.ContainsKey(guild.Id))
                _commands.RemoveModule(_moduleCache[guild.Id]);

            await CreateCommandsAsync(guild);
        }
    }
}
