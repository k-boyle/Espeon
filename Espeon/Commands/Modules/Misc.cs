using Discord;
using Discord.Net;
using Espeon.Interactive.Callbacks;
using Espeon.Interactive.Criteria;
using Espeon.Services;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Espeon.Commands.Modules
{
    //TODO rewrite ping
    /*
     * Cat Fact
     * Joke
     * Chuck
     * Gif
     * Clear Responses
     * Admins
     * Mods
     * Help
     */
    [Name("Misc")]
    public class Misc : EspeonBase
    {
        public Config Config { get; set; }
        public EmotesService Emotes { get; set; }
        public IHttpClientFactory ClientFactory { get; set; }

        private Emoji _delete = new Emoji("🚮");

        [Command("Ping")]
        [Name("Ping")]
        public async Task PingAsync()
        {
            var latency = Context.Client.Latency;
            var str = await Response.GetResponseAsync(Module, Command, ResponsePack, latency, "");
            var response = ResponseBuilder.Message(Context, str);

            var sw = new Stopwatch();
            sw.Start();

            var message = await SendMessageAsync(response);

            sw.Stop();

            str = await Response.GetResponseAsync(Module, Command, ResponsePack, latency, sw.ElapsedMilliseconds);
            response = ResponseBuilder.Message(Context, str);

            await message.ModifyAsync(x => x.Embed = response);

            var deleteCallback = new DeleteCallback(Context, message, _delete,
                new ReactionFromSourceUser(Context.User.Id));

            await TryAddCallbackAsync(deleteCallback);
        }


        //TODO cooldowns
        [Command("Catfact")]
        [RunMode(RunMode.Parallel)]
        [Name("Get Catfact")]
        public async Task GetCatfactAsync()
        {
            var client = ClientFactory.CreateClient("requests");

            using (var response = await client.GetAsync("https://catfact.ninja/fact"))
            {
                if(response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var obj = JObject.Parse(content);

                    await SendOkAsync($"{obj["fact"]}");
                }
                else
                {
                    await SendNotOkAsync("Something went wrong...");
                }
            }
        }

        [Command("Joke")]
        [RunMode(RunMode.Parallel)]
        [Name("Get Joke")]
        public async Task GetJokeAsync()
        {
            var client = ClientFactory.CreateClient("requests");

            using (var response = await client.GetAsync("https://icanhazdadjoke.com/"))
            {
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var obj = JObject.Parse(content);

                    await SendOkAsync($"{obj["joke"]}");
                }
                else
                {
                    await SendNotOkAsync("Something went wrong...");
                }
            }
        }

        [Command("Gif")]
        [RunMode(RunMode.Parallel)]
        [Name("Get Gif")]
        public async Task GetGifAsync([Remainder] string search)
        {
            var client = ClientFactory.CreateClient("requests");

            using (var response = await client.GetAsync($"https://api.giphy.com/v1/gifs/random?api_key={Config.GiphyAPIKey}&rating=r&tag={search}"))
            {
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var obj = JObject.Parse(content);

                    if (!obj["data"].Any())
                    {
                        await SendNotOkAsync("No gif found");
                    }
                    else
                    {
                        var gifUrl = obj["data"]["image_original_url"];

                        using (var stream = await client.GetStreamAsync($"{gifUrl}"))
                        {
                            //TODO MessageService#SendFileAsync

                            try
                            {
                                await Context.Channel.SendFileAsync(stream, $"{search}.gif", string.Empty);
                            }
                            catch(HttpException ex) when(ex.DiscordCode == 40003)
                            {
                                await SendNotOkAsync("The found gif was too big to send");
                            }
                        }
                    }
                }
                else
                {
                    await SendNotOkAsync("Something went wrong...");
                }
            }
        }


        [Command("Clear")]
        [RunMode(RunMode.Parallel)]
        [Name("Clear Messages")]
        public Task ClearMessagesAsync(int amount = 5)
            => Message.DeleteMessagesAsync(Context, amount);

        [Command("help")]
        [Name("Help Modules")]
        public async Task HelpAsync()
        {
            var modules = Services.GetService<CommandService>().GetAllModules();

            var canExecute = new List<Module>();

            foreach(var module in modules)
            {
                var result = await module.RunChecksAsync(Context, Services);

                if(result.IsSuccessful)
                {
                    var commandChecks = module.Commands.Select(x => x.RunChecksAsync(Context, Services));

                    var results = await Task.WhenAll(commandChecks);

                    if (results.Count(x => x.IsSuccessful) > 0)
                        canExecute.Add(module);
                }
            }

            var currentGuild = await Context.Database.GetOrCreateGuildAsync(Context.Guild);
            var prefix = currentGuild.Prefixes.First();

            var builder = GetBuilder(prefix);

            builder.AddField("Modules", string.Join(", ", canExecute.Select(x => $"`{Format.Sanitize(x.Name)}`")));
            builder.WithFooter($"To view help with a specific module invoke {prefix}help Module");

            var message = await SendMessageAsync(builder.Build());

            var deleteCallback = new DeleteCallback(Context, message, _delete,
                new ReactionFromSourceUser(Context.User.Id));

            await TryAddCallbackAsync(deleteCallback);
        }

        [Command("help")]
        [Name("Help Module")]
        public async Task HelpAsync([Remainder] Module module)
        {
            var currentGuild = await Context.Database.GetOrCreateGuildAsync(Context.Guild);
            var prefix = currentGuild.Prefixes.First();

            var canExecute = new List<Command>();

            foreach(var command in module.Commands)
            {
                var result = await command.RunChecksAsync(Context, Services);

                if (result.IsSuccessful)
                    canExecute.Add(command);
            }

            var builder = GetBuilder(prefix);

            builder.AddField("Commands", string.Join(", ", canExecute.Select(x => $"`{Format.Sanitize(x.Name)}`")));
            builder.WithFooter($"To view help with a specific command invoke {prefix}help Command Name");

            var message = await SendMessageAsync(builder.Build());

            var deleteCallback = new DeleteCallback(Context, message, _delete,
                new ReactionFromSourceUser(Context.User.Id));

            await TryAddCallbackAsync(deleteCallback);
        }

        [Command("help")]
        [Name("Help Commands")]
        public async Task HelpAsync([Remainder] IReadOnlyCollection<Command> commands)
        {
            var currentGuild = await Context.Database.GetOrCreateGuildAsync(Context.Guild);
            var prefix = currentGuild.Prefixes.First();

            var builder = GetBuilder(prefix);
            
            builder.WithFooter("You can't go any deeper than this D:");

            foreach(var command in commands)
            {
                //TODO add summaries/examples
                builder.AddField(command.Name, $"{prefix}{command.FullAliases.First()} " +
                    $"{string.Join(' ', command.Parameters.Select(x => $"[{x.Name}]"))}");
            }

            var message = await SendMessageAsync(builder.Build());

            var deleteCallback = new DeleteCallback(Context, message, _delete,
                new ReactionFromSourceUser(Context.User.Id));

            await TryAddCallbackAsync(deleteCallback);
        }

        private EmbedBuilder GetBuilder(string prefix)
        {
            return new EmbedBuilder
            {
                Color = new Color(0xd1a9dd),
                Title = "Espeon's Help",
                Author = new EmbedAuthorBuilder
                {
                    IconUrl = Context.User.GetAvatarOrDefaultUrl(),
                    Name = Context.User.GetDisplayName()
                },
                ThumbnailUrl = Context.Guild.CurrentUser.GetAvatarOrDefaultUrl(),
                Timestamp = DateTimeOffset.UtcNow,
                Description = $"Hello, my name is Espeon{Emotes.Collection["Espeon"]}! " +
                $"You can invoke my commands either by mentioning me or using the `{Format.Sanitize(prefix)}` prefix!"
            };
        }
    }
}
