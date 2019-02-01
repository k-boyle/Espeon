using Discord.WebSocket;
using Espeon.Database.Entities;
using Espeon.Interactive.Criteria;
using Espeon.Services;
using Qmmands;
using System;
using System.Threading.Tasks;
using Espeon.Commands.Checks;

namespace Espeon.Commands.Modules
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
                await SendMessageAsync("What do you want the name to be? Repsond with `cancel` to cancel operation.");

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
                await SendMessageAsync("What do you want the response to be? Repsond with `cancel` to cancel operation.");

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
                await SendOkAsync($"{name} has been successfully created");
                return;
            }

            await SendNotOkAsync("Command already exists");
        }

        [Command("delete")]
        [Name("Delete Command")]
        [RunMode(RunMode.Parallel)]
        public Task DeleteCustomCommandAsync([Remainder] CustomCommand command)
        {
            return Task.WhenAll(Commands.DeleteCommandAsync(Context, command),
                SendOkAsync($"{command.Name} has been deleted"));
        }

        [Command("modify")]
        [Name("Modify Command")]
        [RunMode(RunMode.Parallel)]
        public async Task ModifyCommandAsync(CustomCommand command, [Remainder] string newValue = "")
        {
            if (newValue == "")
            {
                await SendMessageAsync("What do you want the new response to be? Repsond with `cancel` to cancel operation.");

                var reply = await NextMessageAsync(new MultiCriteria<SocketUserMessage>(new UserCriteria(Context.User.Id),
                    new ChannelCriteria(Context.Channel.Id)));

                if (string.Equals(reply.Content, "cancel", StringComparison.InvariantCultureIgnoreCase))
                {
                    return;
                }

                newValue = reply.Content;
            }
            
            await Commands.ModifyCommandAsync(Context, command, newValue);

            await SendOkAsync($"{command.Name} has been updated");
        }
    }
}
