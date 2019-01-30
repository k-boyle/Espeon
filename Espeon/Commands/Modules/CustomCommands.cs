using Discord;
using Discord.WebSocket;
using Espeon.Database.Entities;
using Espeon.Interactive.Criteria;
using Espeon.Services;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Espeon.Commands.Modules
{
    [Name("Custom Commands")]
    [Group("cmd")]
    //TODO elavated user
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

            Embed response;

            if (result)
            {
                response = ResponseBuilder.Message(Context, $"{name} has been successfully created");

                await SendMessageAsync(response);
                return;
            }

            response = ResponseBuilder.Message(Context, "Command already exists", false);

            await SendMessageAsync(response);
        }

        [Command("delete")]
        [Name("Delete Command")]
        [RunMode(RunMode.Parallel)]
        public async Task DeleteCustomCommandAsync([Remainder] CustomCommand command)
        {
            await Commands.DeleteCommandAsync(Context, command);
            var response = ResponseBuilder.Message(Context, $"{command.Name} has been deleted");

            await SendMessageAsync(response);
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

            var response = ResponseBuilder.Message(Context, $"{command.Name} has been updated");

            await SendMessageAsync(response);
        }
    }
}
