using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Net.Helpers;
using MoreLinq;
using Umbreon.Attributes;
using Umbreon.Core;
using Umbreon.Modules.Contexts;
using Umbreon.Modules.ModuleBases;
using Umbreon.Preconditions;

// TODO add require role and finish it all lmao

namespace Umbreon.Modules
{
    [Group("Cmd")]
    [Name("Custom Commands")]
    [Summary("Create custom commands for your server")]
    [RequireEnabled]
    [ModuleType(Module.Commands)]
    [@Remarks("This module can be disabled", "Module Code: Commands")]
    public class CustomCommands : CustomCommandsBase<GuildCommandContext>
    {
        [Command("List", RunMode = RunMode.Async)]
        [Alias("")]
        [Name("List Commands")]
        [Summary("List all the available custom commands for this server")]
        [Usage("cmd list")]
        public async Task ListCmds()
        {
            if (!CurrentCmds.Any())
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
                Color = Color.LightOrange,
                Title = "Available commands for this server",
                Options = new PaginatedAppearanceOptions(),
                Pages = pages
            };
            await SendMessageAsync(null, paginator: paginator);
        }

        [Command("Create", RunMode = RunMode.Async)]
        [Name("Create Command")]
        [Summary("Start the custom command creation process")]
        [Usage("cmd create")]
        public async Task Create()
        {
            await SendMessageAsync("What do you want the command to be called? [reply with `cancel` to cancel creation]");
            var reply = await NextMessageAsync(timeout: TimeSpan.FromSeconds(30));
            if (string.Equals(reply.Content, "cancel", StringComparison.CurrentCultureIgnoreCase)) return;
            var cmdName = reply.Content;
            if (CurrentCmds.Any(x =>
                string.Equals(x.CommandName, cmdName, StringComparison.CurrentCultureIgnoreCase)))
            {
                await SendMessageAsync("This command already exists");
                return;
            }

            if (ReservedWords.Any(x => string.Equals(x, cmdName, StringComparison.CurrentCultureIgnoreCase)))
            {
                await SendMessageAsync("This is a reserved word, command cannot be created");
                return;
            }

            await SendMessageAsync("What do you want the command response to be? [reply with `cancel` to cancel creation]");
            reply = await NextMessageAsync(timeout: TimeSpan.FromSeconds(30));
            if (string.Equals(reply.Content, "cancel", StringComparison.CurrentCultureIgnoreCase)) return;
            var cmdValue = reply.Content;
            await Commands.CreateCmd(Context, cmdName, cmdValue);
            await SendMessageAsync("Command has been created");
        }

        [Command("Create", RunMode = RunMode.Async)]
        [Name("Create Command")]
        [Summary("Creates a command with the specified name")]
        [Usage("cmd create YoutubeUrl")]
        public async Task Create(
            [Name("Command Name")]
                [Summary("The name of the command that you want to create")]string cmdName)
        {
            if (CurrentCmds.Any(x =>
                string.Equals(x.CommandName, cmdName, StringComparison.CurrentCultureIgnoreCase)))
            {
                await SendMessageAsync("This command already exists");
                return;
            }

            if (ReservedWords.Any(x => string.Equals(x, cmdName, StringComparison.CurrentCultureIgnoreCase)))
            {
                await SendMessageAsync("This is a reserved word, command cannot be created");
                return;
            }

            await SendMessageAsync("What do you want the command response to be? [reply with `cancel` to cancel creation]");
            var reply = await NextMessageAsync(timeout: TimeSpan.FromSeconds(30));
            if (string.Equals(reply.Content, "cancel", StringComparison.CurrentCultureIgnoreCase)) return;
            var cmdValue = reply.Content;
            await Commands.CreateCmd(Context, cmdName, cmdValue);
            await SendMessageAsync("Command has been created");
        }

        [Command("Create")]
        [Name("Create Command")]
        [Summary("Creates a command with the pass parameters")]
        [Usage("cmd create YoutubeUrl https://www.youtube.com/")]
        public async Task Create(
            [Name("Command Name")]
                [Summary("The name of the command you want to create")]
                string cmdName,
            [Name("Command Value")]
                [Summary("The response you want from the command")]
                [Remainder]
                string cmdValue)
        {
            if (CurrentCmds.Any(x =>
                string.Equals(x.CommandName, cmdName, StringComparison.CurrentCultureIgnoreCase)))
            {
                await SendMessageAsync("This command already exists");
                return;
            }

            if (ReservedWords.Any(x => string.Equals(x, cmdName, StringComparison.CurrentCultureIgnoreCase)))
            {
                await SendMessageAsync("This is a reserved word, command cannot be created");
                return;
            }

            await Commands.CreateCmd(Context, cmdName, cmdValue);
            await SendMessageAsync("Command has been created");
        }

        [Command("Modify", RunMode = RunMode.Async)]
        [Name("Modify Command")]
        [Summary("Starts the Command modification process")]
        [Usage("cmd modify")]
        public async Task Modify()
        {
            await SendMessageAsync("Which Command do you want to edit? [reply with `cancel` to cancel modification]");
            var reply = await NextMessageAsync(timeout: TimeSpan.FromSeconds(30));
            if (string.Equals(reply.Content, "cancel", StringComparison.CurrentCultureIgnoreCase)) return;

            if (Commands.TryParse(CurrentCmds, reply.Content, out var targetCommand))
            {
                await SendMessageAsync("What do you want the new response to be? [reply with `cancel` to cancel modification]");
                reply = await NextMessageAsync(timeout: TimeSpan.FromSeconds(30));
                if (string.Equals(reply.Content, "cancel", StringComparison.CurrentCultureIgnoreCase)) return;
                var newValue = reply.Content;
                Commands.UpdateCommand(Context, targetCommand.CommandName, newValue);
                await SendMessageAsync("Command has been modified");
                return;
            }

            await SendMessageAsync("Command was not found");
        }

        [Command("Modify", RunMode = RunMode.Async)]
        [Name("Modify Command")]
        [Priority(1)]
        [Summary("Modify the specified Command name")]
        [Usage("cmd modify YoutubeUrl")]
        public async Task Modify(
            [Name("Command Name")]
                [Summary("The Command you wanna modify")]
                [Remainder]string cmdName)
        {
            if (Commands.TryParse(CurrentCmds, cmdName, out var targetCommand))
            {
                await SendMessageAsync("What do you want the new response to be? [reply with `cancel` to cancel modification]");
                var reply = await NextMessageAsync(timeout: TimeSpan.FromSeconds(30));
                if (string.Equals(reply.Content, "cancel", StringComparison.CurrentCultureIgnoreCase)) return;
                var newValue = reply.Content;
                Commands.UpdateCommand(Context, targetCommand.CommandName, newValue);
                await SendMessageAsync("Command has been modified");
                return;
            }

            await SendMessageAsync("Command not found");
        }

        [Command("Modify")]
        [Name("Modify Command")]
        [Summary("Modify the specified Command with the given value")]
        [Usage("cmd modify YoutubeUrl Totally a Url")]
        public async Task Modify(
            [Name("Command Name")]
                [Summary("The name of the Command you want to modify")]
                string cmdName,
            [Name("Command Value")]
                [Summary("The new value that you want the Command to have")]
                [Remainder] string cmdValue)
        {
            if (Commands.TryParse(CurrentCmds, cmdName, out var targetCommand))
            {
                Commands.UpdateCommand(Context, targetCommand.CommandName, cmdValue);
                await SendMessageAsync("Command has been modified");
                return;
            }

            await SendMessageAsync("Command not found");
        }
    }
}
