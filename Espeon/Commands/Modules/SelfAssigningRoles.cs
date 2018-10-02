using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MoreLinq;
using System.Linq;
using System.Threading.Tasks;
using Espeon.Attributes;
using Espeon.Commands.ModuleBases;
using Espeon.Commands.Preconditions;
using Espeon.Core;
using Espeon.Core.Entities.Guild;
using Espeon.Extensions;
using Espeon.Interactive.Paginator;
using Espeon.Services;
using Colour = Discord.Color;

namespace Espeon.Commands.Modules
{
    [Group("Roles")]
    [Name("Self Assigning Roles")]
    [Summary("Roles that users can add/remove to/from themselves")]
    public class SelfAssigningRoles : EspeonBase
    {
        private readonly DatabaseService _database;

        public SelfAssigningRoles(DatabaseService database)
        {
            _database = database;
        }

        [Command("List", RunMode = RunMode.Async)]
        [Alias("")]
        [Name("List Roles")]
        [Summary("List all the available self assigning roles")]
        [Usage("roles list")]
        [Priority(0)]
        public async Task ListRoles()
        {
            var guild = await _database.GetObjectAsync<GuildObject>("guilds", Context.Guild.Id);
            var currentRoles = guild.SelfAssigningRoles;

            if (currentRoles.Count == 0)
            {
                await SendMessageAsync("There are no available self assigning roles");
                return;
            }

            var pages = currentRoles.Select(x => Context.Guild.GetRole(x)).Select(x => x.Name).Batch(10).Select(y => string.Join("\n", y));
            var paginator = new PaginatedMessage
            {
                Author = new EmbedAuthorBuilder
                {
                    IconUrl = Context.User.GetAvatarOrDefaultUrl(),
                    Name = Context.User.GetDisplayName()
                },
                Color = Colour.LightOrange,
                Title = "Available roles for this server",
                Options = new PaginatedAppearanceOptions(),
                Pages = pages
            };
            await SendPaginatedMessageAsync(paginator);
        }

        [Command("Add")]
        [Name("Add Role")]
        [Summary("Add a self assigning role to yourself")]
        [Usage("roles add Espeon")]
        [Priority(1)]
        public async Task AddRole(
            [Name("Role")]
            [Summary("The role you want to add")]
            [Remainder] SocketRole roleToAdd)
        {
            var guild = await _database.GetObjectAsync<GuildObject>("guilds", Context.Guild.Id);
            var currentRoles = guild.SelfAssigningRoles;

            if (currentRoles.Contains(roleToAdd.Id))
            {
                await Context.User.AddRoleAsync(roleToAdd);
                await SendMessageAsync("Role has been added");
                return;
            }

            await SendMessageAsync("Role was not found in the available self assigning roles");
        }

        [Command("Remove")]
        [Name("Remove Role")]
        [Summary("Remove a self assigning role from yourself")]
        [Usage("roles remove Espeon")]
        [Priority(1)]
        public async Task RemoveRole(
            [Name("Role")]
            [Summary("The role you want to remove")]
            [Remainder] SocketRole roleToRemove)
        {
            var guild = await _database.GetObjectAsync<GuildObject>("guilds", Context.Guild.Id);
            var currentRoles = guild.SelfAssigningRoles;

            if (currentRoles.Contains(roleToRemove.Id))
            {
                await Context.User.RemoveRoleAsync(roleToRemove);
                await SendMessageAsync("Role has been removed");
                return;
            }

            await SendMessageAsync("Role was not found in the available self assigning roles");
        }

        [Command("AddSelf")]
        [Name("Add New Role")]
        [Summary("Add a new self assigning role to the server")]
        [Usage("roles addself Espeon")]
        [RequireRole(SpecialRole.Mod, Group = "SpecialRole")]
        [RequireRole(SpecialRole.Admin, Group = "SpecialRole")]
        [Priority(1)]
        public async Task AddNewRole(
            [Name("Role To Add")]
            [Summary("The new role want that you want to add to the self assigning roles")]
            [Remainder] SocketRole roleToAdd)
        {
            var guild = await _database.GetObjectAsync<GuildObject>("guilds", Context.Guild.Id);
            var currentRoles = guild.SelfAssigningRoles;

            if (!currentRoles.Contains(roleToAdd.Id))
            {
                currentRoles.Add(roleToAdd.Id);
                _database.UpdateObject("guilds", guild);
            }

            await SendMessageAsync("New self role has been added");
        }

        [Command("RemoveSelf")]
        [Name("Remove Old Role")]
        [Summary("Add a new self assigning role to the server")]
        [Usage("roles removeself Espeon")]
        [RequireRole(SpecialRole.Mod, Group = "SpecialRole")]
        [RequireRole(SpecialRole.Admin, Group = "SpecialRole")]
        [Priority(1)]
        public async Task RemoveOldRole(
            [Name("Role To Remove")]
            [Summary("The old role want that you want to remove from the self assigning roles")]
            [Remainder] SocketRole roleToRemove)
        {
            var guild = await _database.GetObjectAsync<GuildObject>("guilds", Context.Guild.Id);
            var currentRoles = guild.SelfAssigningRoles;

            if (currentRoles.Contains(roleToRemove.Id))
            {
                currentRoles.Remove(roleToRemove.Id);
                _database.UpdateObject("guilds", guild);
            }

            await SendMessageAsync("Old self role has been removed");
        }
    }
}
