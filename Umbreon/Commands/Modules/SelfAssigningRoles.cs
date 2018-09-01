using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MoreLinq;
using Umbreon.Attributes;
using Umbreon.Commands.Contexts;
using Umbreon.Commands.ModuleBases;
using Umbreon.Commands.Preconditions;
using Umbreon.Core;
using Umbreon.Extensions;
using Umbreon.Interactive.Paginator;
using Umbreon.Services;
using RemarksAttribute = Umbreon.Attributes.RemarksAttribute;
using Colour = Discord.Color;

namespace Umbreon.Commands.Modules
{
    [Group("Roles")]
    [Name("Self Assigning Roles")]
    [Summary("Roles that users can add/remove to/from themselves")]
    [Remarks("This module can be disabled", "Module Code: Roles")]
    [ModuleType(Module.Roles)]
    [RequireEnabled]
    public class SelfAssigningRoles : SelfAssigningRolesBase<UmbreonContext>
    {
        [Command("List", RunMode = RunMode.Async)]
        [Alias("")]
        [Name("List Roles")]
        [Summary("List all the available self assigning roles")]
        [Usage("roles list")]
        [Priority(0)]
        public async Task ListRoles()
        {
            if (CurrentRoles.Count() == 0)
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
        [Usage("roles add Umbreon")]
        [Priority(1)]
        public async Task AddRole(
            [Name("Role")]
            [Summary("The role you want to add")]
            [Remainder] SocketRole roleToAdd)
        {
            if (SelfAssigningRolesService.HasRole(CurrentRoles, roleToAdd.Id))
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
            if (SelfAssigningRolesService.HasRole(CurrentRoles, roleToRemove.Id))
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
        [Usage("roles addself Umbreon")]
        [RequireRole(SpecialRole.Mod, Group = "SpecialRole")]
        [RequireRole(SpecialRole.Admin, Group = "SpecialRole")]
        [Priority(1)]
        public async Task AddNewRole(
            [Name("Role To Add")]
            [Summary("The new role want that you want to add to the self assigning roles")]
            [Remainder] SocketRole roleToAdd)
        {
            if (!SelfAssigningRolesService.HasRole(CurrentRoles, roleToAdd.Id))
            {
                SelfRoles.AddNewSelfRole(Context, roleToAdd.Id);
            }

            await SendMessageAsync("New self role has been added");
        }

        [Command("RemoveSelf")]
        [Name("Remove Old Role")]
        [Summary("Add a new self assigning role to the server")]
        [Usage("roles removeself Umbreon")]
        [RequireRole(SpecialRole.Mod, Group = "SpecialRole")]
        [RequireRole(SpecialRole.Admin, Group = "SpecialRole")]
        [Priority(1)]
        public async Task RemoveOldRole(
            [Name("Role To Remove")]
            [Summary("The old role want that you want to remove from the self assigning roles")]
            [Remainder] SocketRole roleToRemove)
        {
            if (SelfAssigningRolesService.HasRole(CurrentRoles, roleToRemove.Id))
            {
                SelfRoles.RemoveSelfRole(Context, roleToRemove.Id);
            }

            await SendMessageAsync("Old self role has been removed");
        }
    }
}
