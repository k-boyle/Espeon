using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.Net.Helpers;
using Discord.WebSocket;
using MoreLinq;
using Umbreon.Attributes;
using Umbreon.Core;
using Umbreon.Modules.Contexts;
using Umbreon.Modules.ModuleBases;
using Umbreon.Preconditions;

namespace Umbreon.Modules
{
    [Group("Roles")]
    [Name("Self Assigning Roles")]
    [Summary("Roles that users can add/remove to/from themselves")]
    [@Remarks("This module can be disabled", "Module Code: Roles")]
    [ModuleType(Module.Roles)]
    [RequireEnabled]
    public class SelfAssigningRoles : SelfAssigningRolesBase<GuildCommandContext>
    {
        // TODO finish this

        [Command("List", RunMode = RunMode.Async)]
        [Alias("")]
        [Name("List Roles")]
        [Summary("List all the available self assigning roles")]
        [Usage("roles list")]
        [Priority(0)]
        public async Task ListRoles()
        {
            if (!CurrentRoles.Any())
            {
                await SendMessageAsync("There are no available self assigning roles");
                return;
            }

            var pages = CurrentRoles.Select(x => Context.Guild.GetRole(x)).Select(x => x.Name).Batch(10).Select(y => string.Join("\n", y));
            var paginator = new PaginatedMessage
            {
                Author = new EmbedAuthorBuilder
                {
                    IconUrl = Context.User.GetAvatarOrDefaultUrl(),
                    Name = Context.User.GetDisplayName()
                },
                Color = Color.LightOrange,
                Title = "Available roles for this server",
                Options = new PaginatedAppearanceOptions(),
                Pages = pages
            };
            await SendMessageAsync(string.Empty, paginator: paginator);
        }

        [Command("Add")]
        [Name("Add Role")]
        [Summary("Add a self assigning role to yourself")]
        [Usage("roles add Umbreon")]
        [Priority(1)]
        public async Task AddRole(
            [Name("Role")]
            [Summary("The role you want to add")]
            [Remainder] SocketRole roleToAdd)
        {
            if (SelfRoles.HasRole(CurrentRoles, roleToAdd.Id))
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
        [Usage("roles remove Umbreon")]
        [Priority(1)]
        public async Task RemoveRole(
            [Name("Role")]
            [Summary("The role you want to remove")]
            [Remainder] SocketRole roleToRemove)
        {
            if (SelfRoles.HasRole(CurrentRoles, roleToRemove.Id))
            {
                await Context.User.RemoveRoleAsync(roleToRemove);
                await SendMessageAsync("Role has been removed");
                return;
            }

            await SendMessageAsync("Role was not found in the available self assigning roles");
        }
    }
}
