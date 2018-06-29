using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.Net.Helpers;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Addons.Interactive.HelpPaginator;
using Umbreon.Attributes;
using Umbreon.Modules.Contexts;
using Umbreon.Modules.ModuleBases;
using Umbreon.Services;

namespace Umbreon.Modules
{
    [Name("help")]
    public class HelpCommands : UmbreonBase<GuildCommandContext>
    {
        private readonly CommandService _commands;
        private readonly DatabaseService _database;
        private const int BatchSize = 7;

        public HelpCommands(CommandService commands, DatabaseService database)
        {
            _commands = commands;
            _database = database;
        }

        [Command("help")]
        [Priority(0)]
        public async Task HelpCmd()
        {
            var modules = _commands.Modules.Where(x => x.Name != "help" && !x.IsSubmodule).OrderBy(y => y.Name);
            var builder = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    IconUrl = Context.User.GetAvatarOrDefaultUrl(),
                    Name = Context.User.GetDisplayName()
                },
                Color = Color.LightOrange,
                Description = "All the available modules for Umbreon",
                Footer = new EmbedFooterBuilder
                {
                    Text = $"Type {_database.GetGuild(Context).Prefix}help Module-Name to view help for that module"
                },
                ThumbnailUrl = Context.Guild.CurrentUser.GetAvatarOrDefaultUrl(),
                Timestamp = DateTimeOffset.UtcNow,
                Title = "Umbreon's Help"
            };

            builder.AddEmptyField();

            foreach (var mod in modules)
            {
                if(!(await mod.CheckPermissionsAsync(Context)).IsSuccess) continue;

                if (ulong.TryParse(mod.Name, out var id))
                    if (id != Context.Guild.Id)
                        continue;
                    else
                    {
                        builder.AddField(f =>
                        {
                            f.Name = Context.Guild.Name;
                            f.Value = mod.Summary;
                        });
                        continue;
                    }

                builder.AddField(f =>
                {
                    f.Name = mod.Name;
                    f.Value = $"{mod.Summary}";
                });
            }

            await SendMessageAsync(string.Empty, builder.Build());
        }

        [Command("help")]
        [Priority(1)]
        public async Task HelpCmd([Remainder] ModuleInfo module)
        {
            if (string.Equals(module.Name, "help", StringComparison.CurrentCultureIgnoreCase)) return;

            var cmds = module.Commands;
            var pages = cmds.Select(x => $"{x.Name} - `{(x.Attributes.FirstOrDefault(y => y is Usage) as Usage).Example}`").Batch(BatchSize).Select(y => string.Join("\n", y));

            await SendMessageAsync(string.Empty, paginator: new HelpPaginatedMessage
            {
                Author = new EmbedAuthorBuilder
                {
                    IconUrl = Context.User.GetAvatarOrDefaultUrl(),
                    Name = Context.User.GetDisplayName()
                },
                Color = Color.LightOrange,
                Module = module,
                Content = string.Empty,
                Options = PaginatedAppearanceOptions.Default,
                Pages = pages,
                Prefix = _database.GetGuild(Context).Prefix,
                Remarks = (module.Attributes.FirstOrDefault(x => x is @Remarks) as @Remarks)?.RemarkStrings
            });
        }

        [Command("help")]
        [Priority(2)]
        public async Task HelpCmd([Remainder] IEnumerable<CommandInfo> cmds)
        {
            var builder = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    IconUrl = Context.User.GetAvatarOrDefaultUrl(),
                    Name = Context.User.GetDisplayName()
                },
                Color = Color.LightOrange,
                Description = $"All commands found with the name '{cmds.FirstOrDefault().Name}'\n" +
                              $"Type {_database.GetGuild(Context).Prefix}help ... wait no you can't do deeper than this",
                Timestamp = DateTimeOffset.Now, 
                ThumbnailUrl = Context.Guild.CurrentUser.GetAvatarOrDefaultUrl(),
                Title = "Umbreon's Help"
            };

            builder.AddEmptyField();

            foreach (var cmd in cmds)
            {
                builder.AddField(f =>
                {
                    f.Name = $"{cmd.Name}";
                    f.Value = $"**Summary**: {cmd.Summary}\n" +
                              $"**Example Usage**: `{(cmd.Attributes.FirstOrDefault(x => x is Usage) as Usage).Example}`";
                });

                if (cmd.Parameters.Any())
                {
                    builder.AddField(f =>
                    {
                        f.Name = "Parameters";
                        f.Value = $"{string.Join("\n", cmd.Parameters.Select(x => $"`{x.Name}` - {x.Summary}"))}";
                    });
                }

                builder.AddEmptyField();
            }

            await SendMessageAsync(string.Empty, builder.Build());
        }
    }
}
