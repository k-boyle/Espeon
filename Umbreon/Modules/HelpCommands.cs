using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.Net.Helpers;
using MoreLinq;
using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Addons.Interactive.HelpPaginator;
using Umbreon.Attributes;
using Umbreon.Modules.Contexts;
using Umbreon.Services;

namespace Umbreon.Modules
{
    [Name("help")]
    public class HelpCommands : InteractiveBase<GuildCommandContext>
    {
        private readonly CommandService _commands;
        private readonly DatabaseService _database;
        private readonly MessageService _message;

        public HelpCommands(CommandService commands, DatabaseService database, MessageService message)
        {
            _commands = commands;
            _database = database;
            _message = message;
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

            await _message.SendMessageAsync(Context, string.Empty, builder.Build());
        }

        [Command("help")]
        [Priority(1)]
        public async Task HelpCmd([Remainder] ModuleInfo module)
        {
            var cmds = module.Commands;
            var pages = cmds.Select(x => $"{x.Name} - `{(x.Attributes.FirstOrDefault(y => y is Usage) as Usage).Example}`").Batch(5).Select(y => string.Join("\n", y));

            await _message.SendMessageAsync(Context, string.Empty, paginator: new HelpPaginatedMessage
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
                Remarks = (module.Attributes.FirstOrDefault(x => x is @Remarks) as @Remarks).RemarkStrings,
                Title = "placeholder"
            });
        }

        [Command("help")]
        [Priority(1)]
        public async Task HelpCmd([Remainder] CommandInfo cmd)
        {

        }
    }
}
