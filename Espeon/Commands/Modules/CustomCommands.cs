using Discord.WebSocket;
using Espeon.Commands;
using Espeon.Databases.Entities;
using Espeon.Enums;
using Espeon.Commands.Interactive.Criteria;
using Espeon.Services;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Espeon.Commands
{
    [Name("Custom Commands")]
    [Group("cmd")]
    [RequireElevation(ElevationLevel.Mod)]
    public class CustomCommands : EspeonBase
    {
        public CustomCommandsService Commands { get; set; }

        [Command("create")]
        [Name("Create Command")]
        [RunMode(RunMode.Parallel)]
        public async Task CreateCustomCommandAsync(string name = "", [Remainder] string value = "")
        {
            if (name == "")
            {
                await SendOkAsync(0);

                var reply = await NextMessageAsync(new MultiCriteria<SocketUserMessage>(new UserCriteria(Context.User.Id),
                    new ChannelCriteria(Context.Channel.Id)));

                if (string.Equals(reply.Content, "cancel", StringComparison.InvariantCultureIgnoreCase))
                {
                    return;
                }

                name = reply.Content;
            }

            if (value == "")
            {
                await SendOkAsync(1);

                var reply = await NextMessageAsync(new MultiCriteria<SocketUserMessage>(new UserCriteria(Context.User.Id),
                    new ChannelCriteria(Context.Channel.Id)));

                if (string.Equals(reply.Content, "cancel", StringComparison.InvariantCultureIgnoreCase))
                {
                    return;
                }

                value = reply.Content;
            }

            var result = await Commands.TryCreateCommandAsync(Context, name, value);

            if (result)
            {
                await SendOkAsync(2, name);
                return;
            }

            await SendNotOkAsync(3);
        }

        [Command("delete")]
        [Name("Delete Command")]
        [RunMode(RunMode.Parallel)]
        public async Task DeleteCustomCommandAsync([Remainder] CustomCommand command)
        {
            await Commands.DeleteCommandAsync(Context, command);
            await SendOkAsync(0, command.Name);
        }

        [Command("modify")]
        [Name("Modify Command")]
        [RunMode(RunMode.Parallel)]
        public async Task ModifyCommandAsync(CustomCommand command, [Remainder] string newValue = "")
        {
            if (newValue == "")
            {
                await SendOkAsync(0);

                var reply = await NextMessageAsync(new MultiCriteria<SocketUserMessage>(new UserCriteria(Context.User.Id),
                    new ChannelCriteria(Context.Channel.Id)));

                if (string.Equals(reply.Content, "cancel", StringComparison.InvariantCultureIgnoreCase))
                {
                    return;
                }

                newValue = reply.Content;
            }
            
            await Commands.ModifyCommandAsync(Context, command, newValue);

            await SendOkAsync(1, command.Name);
        }
    }
}
