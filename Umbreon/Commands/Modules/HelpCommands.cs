using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Umbreon.Attributes;
using Umbreon.Commands.ModuleBases;
using Umbreon.Core.Entities.Guild;
using Umbreon.Extensions;
using Umbreon.Helpers;
using Umbreon.Services;
using Colour = Discord.Color;
using Remarks = Umbreon.Attributes.RemarksAttribute;

namespace Umbreon.Commands.Modules
{
    [Name("Help")]
    public class HelpCommands : UmbreonBase
    {
        private readonly CommandService _commands;
        private readonly DatabaseService _database;
        private string Prefix => _database.GetObject<GuildObject>("guilds", Context.Guild.Id).Prefixes.First();

        public HelpCommands(CommandService commands, DatabaseService database)
        {
            _commands = commands;
            _database = database;
        }

        [Command("help")]
        public async Task Help()
        {
            var modules = _commands.Modules.Where(x => x.Name != "Help");
            var canExecute = new List<ModuleInfo>();
            foreach (var module in modules)
                if ((await module.CheckPermissionsAsync(Context, Services)).IsSuccess)
                    canExecute.Add(module);

            var builder = Embed();
            builder.WithFooter($"You can view help on a specific module by doing {Prefix}help module");
            builder.AddField("Modules", string.Join(", ", canExecute.Select(x => $"`{Format.Sanitize(x.Name)}`")));

            await (await SendMessageAsync(string.Empty, embed: builder.Build())).AddDeleteCallbackAsync(Context, Interactive);
        }

        [Command("help")]
        [Priority(1)]
        public async Task Help([Remainder] ModuleInfo module)
        {
            var commands = module.Commands;
            var canExecute = new List<CommandInfo>();
            foreach (var command in commands)
                if ((await command.CheckPreconditionsAsync(Context, Services)).IsSuccess)
                    canExecute.Add(command);

            if (canExecute.Count == 0)
            {
                await SendMessageAsync("You can't execute any commands in this module");
                return;
            }

            var remarks = module.Attributes.OfType<Remarks>().FirstOrDefault();
            var builder = Embed();
            builder.WithFooter($"You can view help on a specific command by doing {Prefix}help command");
            builder.AddField($"{module.Name} Information", $"**Summary**: {module.Summary}" +
                                                           $"{(remarks is null ? "" : $"\n**Remarks**: {string.Join(", ", remarks.RemarkStrings)}")}");
            builder.AddField("Commands", string.Join(", ", canExecute.Select(x => $"`{Format.Sanitize(x.Aliases.FirstOrDefault())}`")));

            await (await SendMessageAsync(string.Empty, embed: builder.Build())).AddDeleteCallbackAsync(Context, Interactive);
        }

        [Command("help")]
        [Priority(2)]
        public async Task Help([Remainder] IEnumerable<CommandInfo> commands)
        {
            var builder = Embed();
            builder.WithFooter($"We must go deeper! {Prefix}deeper command");

            foreach (var command in commands)
            {
                var usage = command.Attributes.OfType<UsageAttribute>().FirstOrDefault();
                var remarks = command.Attributes.OfType<Remarks>().FirstOrDefault();

                builder.AddField(command.Name, $"**Usage**: {Prefix}{usage?.Example}\n" +
                                               $"**Summary**: {command.Summary}" +
                                               $"{(remarks is null ? "" : $"\n**Remarks**: {string.Join(", ", remarks.RemarkStrings)}\n")}" +
                                               $"{(command.Aliases.Count > 1 ? $"\n**Aliases**: {string.Join(", ", command.Aliases.Select(x => $"`{Format.Sanitize(x)}`"))}" : "")}");
            }

            await (await SendMessageAsync(string.Empty, embed: builder.Build())).AddDeleteCallbackAsync(Context, Interactive);
        }

        [Command("deeper")]
        public async Task Deeper([Remainder] IEnumerable<CommandInfo> commands)
        {
            var builder = Embed();
            builder.WithFooter("You're at the core... Going deeper would just be going back");

            foreach (var command in commands)
            {
                builder.AddField(command.Name, $"**Usage**: {_database.GetObject<GuildObject>("guilds", Context.Guild.Id).Prefixes.First()}{command.Attributes.OfType<UsageAttribute>().Single().Example}\n" +
                                               $"**Summary**: {command.Summary}\n" +
                                               $"**Parameter**: {string.Join("\n**Parameter**: ", command.Parameters.Select(x => $"`{x.Name}` - {x.Summary}"))}");
            }

            await (await SendMessageAsync(string.Empty, embed: builder.Build())).AddDeleteCallbackAsync(Context, Interactive);
        }

        private EmbedBuilder Embed()
            => new EmbedBuilder
            {
                Color = new Colour(255, 255, 39),
                Title = "Umbreon's help",
                Author = new EmbedAuthorBuilder
                {
                    IconUrl = Context.User.GetAvatarOrDefaultUrl(),
                    Name = Context.User.GetDisplayName()
                },
                ThumbnailUrl = Context.Guild.CurrentUser.GetAvatarOrDefaultUrl(),
                Timestamp = DateTimeOffset.UtcNow,
                Description = $"Hello, my name is Umbreon{EmotesHelper.Emotes["umbreon"]}! You can invoke my commands either by mentioning me or using the `{Format.Sanitize(Prefix)}` prefix!"
            };
    }
}
