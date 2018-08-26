using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Umbreon.Attributes;
using Umbreon.Commands.ModuleBases;
using Umbreon.Commands.TypeReaders;
using Umbreon.Services;
using Activity = Umbreon.Core.Entities.Activity;

namespace Umbreon.Commands.Modules
{
    [Name("Owner Commands")]
    [Summary("Super duper secret commands")]
    [RequireOwner]
    public class OwnerModule : UmbreonBase
    {
        private readonly EvalService _eval;

        public OwnerModule(EvalService eval)
        {
            _eval = eval;
        }

        [Command("activity")]
        [Name("Set Activity")]
        [Summary("Change what activity the bot is doing")]
        [Usage("activity watching")]
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
        [Usage("username Umbreon")]
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
        [Usage("message 123 hi there")]
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

        [Command("eval", RunMode = RunMode.Async)]
        [Name("Eval")]
        [Summary("Evaluate C# code")]
        [Usage("eval Console.WriteLine(\"Umbreon is the bestest\");")]
        public Task Eval(
            [Name("Code")]
            [Summary("The code you want to evaluate")]
            [Remainder]
            [OverrideTypeReader(typeof(CodeTypeReader))]
            string code)
            => _eval.EvaluateAsync(Context, code, true, Services);

        [Command("flush")]
        [Name("Flush Reactions")]
        [Summary("Remove all callbacks from interactive")]
        [Usage("flush")]
        public Task Flush()
        {
            Interactive.ClearReactionCallbacks();
            return Task.CompletedTask;
        }

        [Command("gc")]
        [Name("Garbage Collect")]
        [Summary("Run garbage collection")]
        [Usage("gc")]
        public Task GC()
        {
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            return Task.CompletedTask;
        }
    }
}
