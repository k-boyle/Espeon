using Discord;
using Discord.Commands;
using MoreLinq;
using System;
using System.Linq;
using System.Threading.Tasks;
using Umbreon.Attributes;
using Umbreon.Commands.Contexts;
using Umbreon.Commands.ModuleBases;
using Umbreon.Commands.Preconditions;
using Umbreon.Core;
using Umbreon.Core.Entities.Guild;
using Umbreon.Extensions;
using Umbreon.Interactive.Paginator;
using Umbreon.Services;
using Colour = Discord.Color;
using RemarksAttribute = Umbreon.Attributes.RemarksAttribute;

namespace Umbreon.Commands.Modules
{
    [Group("Cmd")]
    [Name("Custom Commands")]
    [Summary("Create custom commands for your server")]
    [Remarks("This module can be disabled", "Module Code: Commands")]
    public class CustomCommands : CustomCommandsBase<UmbreonContext>
    {
        [Command("List", RunMode = RunMode.Async)]
        [Alias("")]
        [Name("List Commands")]
        [Summary("List all the available custom commands for this server")]
        [Priority(0)]
        [Usage("cmd list")]
        public async Task ListCmds()
        {
            if (CurrentCmds.Count() == 0)
            {
                await SendMessageAsync("No custom commands currently for this server");
                return;
            }

            var pages = CurrentCmds.Select(x => x.CommandName).Batch(10).Select(y => string.Join("\n", y));
            var paginator = new PaginatedMessage
            {
                Author = new EmbedAuthorBuilder
                {
                    IconUrl = Context.User.GetAvatarOrDefaultUrl(),
                    Name = Context.User.GetDisplayName()
                },
                Color = Colour.LightOrange,
                Title = "Available commands for this server",
                Options = new PaginatedAppearanceOptions(),
                Pages = pages
            };
            await SendPaginatedMessageAsync(paginator);
        }

        [Command("Create", RunMode = RunMode.Async)]
        [Name("Create Command")]
        [Summary("Start the custom command creation process")]
        [Usage("cmd create")]
        [Priority(1)]
        [RequireRole(SpecialRole.Admin, Group = "admin")]
        [RequireOwner(Group = "admin")]
        public async Task Create()
        {
            await SendMessageAsync("What do you want the command to be called? [reply with `cancel` to cancel creation]");
            var reply = await NextMessageAsync(timeout: TimeSpan.FromSeconds(30));
            if (string.Equals(reply.Content, "cancel", StringComparison.CurrentCultureIgnoreCase)) return;
            var cmdName = reply.Content;
            if (CurrentCmds.Any(x =>
                string.Equals(x.CommandName, cmdName, StringComparison.CurrentCultureIgnoreCase)))
            {
                await NewMessageAsync("This command already exists");
                return;
            }

            if (ReservedWords.Any(x => string.Equals(x, cmdName, StringComparison.CurrentCultureIgnoreCase)) || Commands.IsReserved(cmdName))
            {
                await NewMessageAsync("This is a reserved word, command cannot be created");
                return;
            }

            await NewMessageAsync("What do you want the command response to be? [reply with `cancel` to cancel creation]");
            reply = await NextMessageAsync(timeout: TimeSpan.FromSeconds(30));
            if (string.Equals(reply.Content, "cancel", StringComparison.CurrentCultureIgnoreCase)) return;
            var cmdValue = reply.Content;
            await Commands.CreateCmdAsync(Context, cmdName, cmdValue);
            await NewMessageAsync("Command has been created");
        }

        [Command("Create", RunMode = RunMode.Async)]
        [Name("Create Command")]
        [Summary("Creates a command with the specified name")]
        [Usage("cmd create YoutubeUrl")]
        [Priority(1)]
        [RequireRole(SpecialRole.Admin, Group = "admin")]
        [RequireOwner(Group = "admin")]
        public async Task Create(
            [Name("Command Name")]
            [Summary("The name of the command that you want to create")] string cmdName)
        {
            if (CurrentCmds.Any(x =>
                string.Equals(x.CommandName, cmdName, StringComparison.CurrentCultureIgnoreCase)))
            {
                await SendMessageAsync("This command already exists");
                return;
            }

            if (ReservedWords.Any(x => string.Equals(x, cmdName, StringComparison.CurrentCultureIgnoreCase)) || Commands.IsReserved(cmdName))
            {
                await SendMessageAsync("This is a reserved word, command cannot be created");
                return;
            }

            await SendMessageAsync("What do you want the command response to be? [reply with `cancel` to cancel creation]");
            var reply = await NextMessageAsync(timeout: TimeSpan.FromSeconds(30));
            if (string.Equals(reply.Content, "cancel", StringComparison.CurrentCultureIgnoreCase)) return;
            var cmdValue = reply.Content;
            await Commands.CreateCmdAsync(Context, cmdName, cmdValue);
            await NewMessageAsync("Command has been created");
        }

        [Command("Create")]
        [Name("Create Command")]
        [Summary("Creates a command with the pass parameters")]
        [Usage("cmd create YoutubeUrl https://www.youtube.com/")]
        [Priority(1)]
        [RequireRole(SpecialRole.Admin, Group = "admin")]
        [RequireOwner(Group = "admin")]
        public async Task Create(
            [Name("Command Name")]
            [Summary("The name of the command you want to create")] string cmdName,
            [Name("Command Value")]
            [Summary("The response you want from the command")]
            [Remainder] string cmdValue)
        {
            if (CurrentCmds.Any(x =>
                string.Equals(x.CommandName, cmdName, StringComparison.CurrentCultureIgnoreCase)))
            {
                await SendMessageAsync("This command already exists");
                return;
            }

            if (ReservedWords.Any(x => string.Equals(x, cmdName, StringComparison.CurrentCultureIgnoreCase)) || Commands.IsReserved(cmdName))
            {
                await SendMessageAsync("This is a reserved word, command cannot be created");
                return;
            }

            await Commands.CreateCmdAsync(Context, cmdName, cmdValue);
            await SendMessageAsync("Command has been created");
        }

        [Command("Modify", RunMode = RunMode.Async)]
        [Name("Modify Command")]
        [Summary("Starts the Command modification process")]
        [Usage("cmd modify")]
        [Priority(1)]
        [RequireRole(SpecialRole.Admin, Group = "admin")]
        [RequireOwner(Group = "admin")]
        public async Task Modify()
        {
            await SendMessageAsync("Which Command do you want to edit? [reply with `cancel` to cancel modification]");
            var reply = await NextMessageAsync(timeout: TimeSpan.FromSeconds(30));
            if (string.Equals(reply.Content, "cancel", StringComparison.CurrentCultureIgnoreCase)) return;

            if (CustomCommandsService.TryParse(CurrentCmds, reply.Content, out var targetCommand))
            {
                await NewMessageAsync("What do you want the new response to be? [reply with `cancel` to cancel modification]");
                reply = await NextMessageAsync(timeout: TimeSpan.FromSeconds(30));
                if (string.Equals(reply.Content, "cancel", StringComparison.CurrentCultureIgnoreCase)) return;
                var newValue = reply.Content;
                Commands.UpdateCommand(Context, targetCommand.CommandName, newValue);
                await NewMessageAsync("Command has been modified");
                return;
            }

            await SendMessageAsync("Command was not found");
        }

        [Command("Modify", RunMode = RunMode.Async)]
        [Name("Modify Command")]
        [Summary("Modify the specified Command name")]
        [Usage("cmd modify YoutubeUrl")]
        [Priority(1)]
        [RequireRole(SpecialRole.Admin, Group = "admin")]
        [RequireOwner(Group = "admin")]
        public async Task Modify(
            [Name("Command Name")]
            [Summary("The Command you wanna modify")] CustomCommand cmd)
        {
            await SendMessageAsync("What do you want the new response to be? [reply with `cancel` to cancel modification]");
            var reply = await NextMessageAsync(timeout: TimeSpan.FromSeconds(30));
            if (string.Equals(reply.Content, "cancel", StringComparison.CurrentCultureIgnoreCase)) return;
            var newValue = reply.Content;
            Commands.UpdateCommand(Context, cmd.CommandName, newValue);
            await NewMessageAsync("Command has been modified");
        }

        [Command("Modify")]
        [Name("Modify Command")]
        [Summary("Modify the specified Command with the given value")]
        [Usage("cmd modify YoutubeUrl Totally a Url")]
        [Priority(1)]
        [RequireRole(SpecialRole.Admin, Group = "admin")]
        [RequireOwner(Group = "admin")]
        public async Task Modify(
            [Name("Command Name")]
            [Summary("The name of the Command you want to modify")] CustomCommand cmd,
            [Name("Command Value")]
            [Summary("The new value that you want the Command to have")]
            [Remainder] string cmdValue)
        {
            Commands.UpdateCommand(Context, cmd.CommandName, cmdValue);
            await SendMessageAsync("Command has been modified");
        }

        [Command("Remove", RunMode = RunMode.Async)]
        [Name("Remove Command")]
        [Summary("Remove a custom command from the server")]
        [Usage("cmd remove")]
        [Priority(1)]
        [RequireRole(SpecialRole.Admin, Group = "admin")]
        [RequireOwner(Group = "admin")]
        public async Task Remove()
        {
            await SendMessageAsync("Which Command do you want to remove? [reply with `cancel` to cancel modification]");
            var reply = await NextMessageAsync(timeout: TimeSpan.FromSeconds(30));
            if (string.Equals(reply.Content, "cancel", StringComparison.CurrentCultureIgnoreCase)) return;

            if (CustomCommandsService.TryParse(CurrentCmds, reply.Content, out var targetCommand))
            {
                await Commands.RemoveCmdAsync(Context, targetCommand.CommandName);
                await NewMessageAsync("Command has been removed");
                return;
            }

            await NewMessageAsync("Command was not found");
        }

        [Command("Remove")]
        [Name("Remove Command")]
        [Summary("Remove the passed custom command from the server")]
        [Usage("cmd remove YoutubeUrl")]
        [Priority(1)]
        [RequireRole(SpecialRole.Admin, Group = "admin")]
        [RequireOwner(Group = "admin")]
        public async Task Remove(
            [Name("Command Name")]
            [Summary("The name of the command that you want to remove")]
            [Remainder] CustomCommand cmd)
        {
            await Commands.RemoveCmdAsync(Context, cmd.CommandName);
            await SendMessageAsync("Command has been removed");
        }
    }
}
