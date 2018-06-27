using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;
using Umbreon.Attributes;
using Umbreon.Core;
using Umbreon.Core.Extensions;
using Umbreon.Modules.ModuleBases;
using Umbreon.Preconditions;

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
        [Group("Create")]
        [Name("Create Command")]
        [Summary("Create a custom command for your server")]
        public class CreateCmd : CustomCommands
        {
            [Command(RunMode = RunMode.Async)]
            [Name("Create Command")]
            [Summary("Start the custom command creation process")]
            [Usage("cmd create")]
            public async Task Create()
            {
                await Message.SendMessageAsync(Context, "What do you want the command to be called? [reply with `cancel` to cancel creation]");
                var reply = await NextMessageAsync(timeout: TimeSpan.FromSeconds(30));
                if (string.Equals(reply.Content, "cancel", StringComparison.CurrentCultureIgnoreCase)) return;
                var cmdName = reply.Content;
                if (CurrentCmds.Any(x => string.Equals(x.CommandName, cmdName, StringComparison.CurrentCultureIgnoreCase)))
                {
                    await Message.SendMessageAsync(Context, "This command already exists");
                    return;
                }

                if (ReservedWords.Any(x => string.Equals(x, cmdName, StringComparison.CurrentCultureIgnoreCase)))
                {
                    await Message.SendMessageAsync(Context, "This is a reserved word, command cannot be created");
                    return;
                }

                await Message.SendMessageAsync(Context, "What do you want the command response to be? [reply with `cancel` to cancel creation]");
                reply = await NextMessageAsync(timeout: TimeSpan.FromSeconds(30));
                if (string.Equals(reply.Content, "cancel", StringComparison.CurrentCultureIgnoreCase)) return;
                var cmdValue = reply.Content;
                Commands.CreateCmd(Context, cmdName, cmdValue);
                await Message.SendMessageAsync(Context, "Command has been created");
            }
        }

        [Command(RunMode = RunMode.Async)]
        [Name("Create Command")]
        [Summary("Creates a command with the specified name")]
        [Usage("cmd create YoutubeUrl")]
        public async Task Create(
            [Name("Command Name")]
            [Summary("The name of the command that you want to create")]
            [Remainder]string cmdName)
        {
            if (CurrentCmds.Any(x => string.Equals(x.CommandName, cmdName, StringComparison.CurrentCultureIgnoreCase)))
            {
                await Message.SendMessageAsync(Context, "This command already exists");
                return;
            }

            if (ReservedWords.Any(x => string.Equals(x, cmdName, StringComparison.CurrentCultureIgnoreCase)))
            {
                await Message.SendMessageAsync(Context, "This is a reserved word, command cannot be created");
                return;
            }

            await Message.SendMessageAsync(Context, "What do you want the command response to be? [reply with `cancel` to cancel creation]");
            var reply = await NextMessageAsync(timeout: TimeSpan.FromSeconds(30));
            if (string.Equals(reply.Content, "cancel", StringComparison.CurrentCultureIgnoreCase)) return;
            var cmdValue = reply.Content;
            await Commands.CreateCmd(Context, cmdName, cmdValue);
            await Message.SendMessageAsync(Context, "Command has been created");
        }

        [Command]
        [Name("Create Command")]
        [Summary("Creates a command with the pass parameters")]
        [Usage("tag create YoutubeUrl https://www.youtube.com/")]
        public async Task Create(
            [Name("Command Name")]
            [Summary("The name of the command you want to create")]
            string cmdName,
            [Name("Command Value")]
            [Summary("The response you want from the command")]
            [Remainder] string cmdValue)
        {
            if (CurrentCmds.Any(x => string.Equals(x.CommandName, cmdName, StringComparison.CurrentCultureIgnoreCase)))
            {
                await Message.SendMessageAsync(Context, "This command already exists");
                return;
            }

            if (ReservedWords.Any(x => string.Equals(x, cmdName, StringComparison.CurrentCultureIgnoreCase)))
            {
                await Message.SendMessageAsync(Context, "This is a reserved word, command cannot be created");
                return;
            }

            await Commands.CreateCmd(Context, cmdName, cmdValue);
            await Message.SendMessageAsync(Context, "Command has been created");
        }
    }
}
