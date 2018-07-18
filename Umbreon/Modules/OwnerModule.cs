using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System.Diagnostics;
using System.Threading.Tasks;
using Umbreon.Attributes;
using Umbreon.Helpers;
using Umbreon.Modules.Contexts;
using Umbreon.Modules.ModuleBases;
using Umbreon.TypeReaders;
using Activity = Umbreon.Activities.Activity;

namespace Umbreon.Modules
{
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

        [Command("eval", RunMode = RunMode.Async)]
        [Name("Eval")]
        [Summary("Evaluate C# code")]
        [Usage("cas eval Console.WriteLine(\"Umbreon is the bestest\");")]
        public async Task Eval(
            [Name("Code")]
            [Summary("The code you want to evaluate")]
            [Remainder]
            [OverrideTypeReader(typeof(CodeTypeReader))] string code)
        {
            var scriptOptions = ScriptOptions.Default.AddEssemblies().AddNamespaces();
            var sw = new Stopwatch();
            var message = await SendMessageAsync("Debugging... ");
            var global = new Globals
            {
                Context = Context
            };
            sw.Start();
            try
            {
                var eval = await CSharpScript.EvaluateAsync(code, scriptOptions, global, typeof(Globals));
                sw.Stop();
                await message.ModifyAsync(x => x.Content = $"Completed! Time taken: {sw.ElapsedMilliseconds}ms\n" +
                                                           $"Returned Results: {eval ?? "None"}");
            }
            catch (CompilationErrorException e)
            {
                await message.ModifyAsync(x => x.Content = $"Completed! But there was an error:\n{e.Message}");
            }
        }
    }
}
