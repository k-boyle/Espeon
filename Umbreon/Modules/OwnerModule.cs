using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using Umbreon.Activities;
using Umbreon.Attributes;
using Umbreon.Modules.Contexts;
using Umbreon.Modules.ModuleBases;

namespace Umbreon.Modules
{
    // TODO eval

    [Group("cas")]
    [Name("Owner Commands")]
    [Summary("Super duper secret commands")]
    [RequireOwner]
    public class OwnerModule : UmbreonBase<GuildCommandContext>
    {
        [Command("activity")]
        [Name("Set Activity")]
        [Summary("Change what activity the bot is doing")]
        [Usage("cas activity watching")]
        public async Task SetPlaying(
            [Name("Activity")]
            [Summary("Listening, Streaming, Playing, Watching")]
            ActivityType activity, 
            [Name("Playing")]
            [Summary("What you want the playing message to be")]
            [Remainder] string playing = "")
        {
            await Context.Client.SetActivityAsync(new Activity(playing, activity));
            await SendMessageAsync("Activity has been set");
        }

        [Command("Username")]
        [Name("Set Username")]
        [Summary("Change the bots username")]
        [Usage("cas username Umbreon")]
        public async Task SetUsername(
            [Name("New Name")]
            [Summary("The new username for the bot")]
            [Remainder] string userName)
        {
            await Context.Client.CurrentUser.ModifyAsync(x => x.Username = userName);
            await SendMessageAsync("Username has been changed");
        }

        [Command("Message")]
        [Name("Message Guild")]
        [Summary("Send a message to the passed channel in a different guild")]
        [Usage("cas message 123 hi there")]
        public async Task SendMessage(
            [Name("Channel Id")]
            [Summary("Id of the channel you want to send the message to")] ulong channelId,
            [Name("Message")]
            [Summary("The message you want to send")]
            [Remainder] string message)
        {
            var channel = Context.Client.GetChannel(channelId) as SocketTextChannel;
            await channel.SendMessageAsync(message);
        }
    }
}
