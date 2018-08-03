using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Umbreon.Attributes;
using Umbreon.Extensions;
using Umbreon.Interactive.Paginator;
using Umbreon.Modules.Contexts;
using Umbreon.Modules.ModuleBases;
using Umbreon.Paginators.HelpPaginator;
using Umbreon.Services;
using RemarksAttribute = Umbreon.Attributes.RemarksAttribute;

namespace Umbreon.Modules
{
    [Name("Help")]
    [Summary("Help commands")]
    public class HelpCommands : UmbreonBase<GuildCommandContext>
    {
        private readonly CommandService _commands;
        private readonly DatabaseService _database;
        private readonly MessageService _messageService;

        public HelpCommands(CommandService commands, DatabaseService database, MessageService messageService)
        {
            _commands = commands;
            _database = database;
            _messageService = messageService;
        }

        [Command("help")]
        public async Task HelpCmd()
        {
            var modules = _commands.Modules.Where(x => !string.Equals(x.Name, "help", StringComparison.CurrentCultureIgnoreCase)).OrderBy(y => y.Name);
            var pages = new List<Page>();
            var fields = new List<EmbedFieldBuilder>();
            Page newPage;
            foreach (var module in modules)
            {
                var remarks = module.Attributes.FirstOrDefault(x => x is RemarksAttribute) as RemarksAttribute;
                fields.Clear();
                newPage = new Page
                {
                    Fields = new List<EmbedFieldBuilder>()
                };
                if(!(await module.CheckPermissionsAsync(Context, module.Commands.FirstOrDefault(), Services)).IsSuccess) continue;

                newPage.Title = new EmbedFieldBuilder
                {
                    Name = $"Module: {(ulong.TryParse(module.Name, out _) ? Context.Guild.Name : module.Name)}",
                    Value = $"**Summary**: {module.Summary}\n" +
                            $"{(!(remarks is null) ? $"**Remark**:{string.Join("\n**Remark**:", remarks.RemarkStrings)}" : "")}\n"
                };

                foreach (var cmd in module.Commands)
                {
                    if(!(await cmd.CheckPreconditionsAsync(Context, Services)).IsSuccess) continue;
                    
                    newPage.Fields.Add(new EmbedFieldBuilder
                    {
                        Name = $"Command: {cmd.Name}",
                        Value = $"**Summary**: {cmd.Summary}\n" +
                                $"**Example Usage**: {_database.GetGuild(Context).Prefixes.First()}{(cmd.Attributes.FirstOrDefault(x => x is UsageAttribute) as UsageAttribute).Example}"
                    });
                }

                pages.Add(newPage);
            }

            var paginatedMessage = new HelpPaginatedMessage
            {
                Author = new EmbedAuthorBuilder
                {
                    IconUrl = Context.User.GetAvatarOrDefaultUrl(),
                    Name = Context.User.GetDisplayName()
                },
                Options = PaginatedAppearanceOptions.Default,
                Pages = pages,
                Prefix = _database.GetGuild(Context).Prefixes.First(),
            };

            //await SendMessageAsync(string.Empty, paginator: paginatedMessage);

            await _messageService.SendPaginatedMessageAsync(Context, paginatedMessage);
        }

        [Command("help")]
        public async Task Help([Remainder] IEnumerable<CommandInfo> commands)
        {
            var filtered = commands.Where(x => !string.Equals(x.Name, "help", StringComparison.CurrentCultureIgnoreCase));
            if (!filtered.Any()) return;
            var results = new List<CommandInfo>();
            foreach (var cmd in filtered)
            {
                if (!(await cmd.CheckPreconditionsAsync(Context, Services)).IsSuccess) continue;
                results.Add(cmd);
            }

            if (!results.Any()) return;

            var builder = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    IconUrl = Context.User.GetAvatarOrDefaultUrl(),
                    Name = Context.User.GetDisplayName()
                },
                Color = Color.Gold,
                Timestamp = DateTimeOffset.UtcNow
            };

            foreach (var cmd in results)
            {
                builder.AddField(f =>
                {
                    f.Name = cmd.Name;
                    f.Value = $"**Summary**: {cmd.Summary}\n" +
                              $"**Example Usage**: {_database.GetGuild(Context).Prefixes.First()}{(cmd.Attributes.FirstOrDefault(x => x is UsageAttribute) as UsageAttribute).Example}\n" +
                              $"**Parameter**: {string.Join("\n**Parameter**:", cmd.Parameters.Select(x => $"`{x.Name}` > {x.Summary}"))}";
                });
            }

            await SendMessageAsync(string.Empty, embed: builder.Build());
        }
    }
}
