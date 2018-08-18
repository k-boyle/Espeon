using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Umbreon.Attributes;
using Umbreon.Helpers;
using Umbreon.Modules.ModuleBases;
using Umbreon.Preconditions;

namespace Umbreon.Modules
{
    [Name("Fun Commands")]
    [Summary("Umbreon has a fun side")]
    public class FunCommands : UmbreonBase
    {
        [Command("Catfact", RunMode = RunMode.Async)]
        [Name("Catfact")]
        [Summary("Grabs a random catfact")]
        [Ratelimit(1, 10, Measure.Seconds)]
        [Usage("catfact")]
        public async Task GetCatfact()
        {
            var msg = await SendMessageAsync("Fetching...");
            var fact = (await SendRequest("https://catfact.ninja/fact"))["fact"];
            await msg.ModifyAsync(x => x.Content = $"{fact}");
        }

        [Command("Joke", RunMode = RunMode.Async)]
        [Name("Joke")]
        [Summary("Grabs a random joke")]
        [Ratelimit(1, 10, Measure.Seconds)]
        [Usage("joke")]
        public async Task GetJoke()
        {
            var msg = await SendMessageAsync("Fetching...");
            var joke = (await SendRequest("https://icanhazdadjoke.com/"))["joke"];
            await msg.ModifyAsync(x => x.Content = $"{joke}");
        }

        [Command("Chuck", RunMode = RunMode.Async)]
        [Name("Chuck")]
        [Summary("Grabs a random Chuck Norris 'joke'")]
        [Ratelimit(1, 10, Measure.Seconds)]
        [Usage("chuck")]
        public async Task GetChuck()
        {
            var msg = await SendMessageAsync("Fetching...");
            var joke = (await SendRequest("http://api.icndb.com/jokes/random"))["value"]["joke"];
            await msg.ModifyAsync(x => x.Content = $"{joke}");
        }

        [Command("gif", RunMode = RunMode.Async)]
        [Name("Gif")]
        [Summary("Gets a random gif")]
        [Ratelimit(1, 10, Measure.Seconds)]
        [Usage("gif party")]
        public async Task GetGif(
            [Name("Search")]
            [Summary("Your search parameter")]
            [Remainder] string search)
        {
            var msg = await SendMessageAsync("Fetching...");
            var req = await SendRequest($"https://api.giphy.com/v1/gifs/random?api_key={ConstantsHelper.GiphyToken}&rating=r&tag={search.Replace(" ", " + ")}");
            if (!req["data"].Any())
            {
                await msg.ModifyAsync(x => x.Content = "No gif found");
                return;
            }

            var gif = req["data"]["image_original_url"];
            var stream = await GetStream($"{gif}");
            await msg.DeleteAsync();
            await Context.Channel.SendFileAsync(stream, "gif.gif", string.Empty);
            Stream.Dispose();
        }
    }
}
