using Discord;
using Discord.Net;
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

namespace Espeon.Commands
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

        private readonly Emoji _delete = new Emoji("🚮");

        //TODO this is a special case
        [Command("Ping")]
        [Name("Ping")]
        public async Task PingAsync()
        {
            var latency = Context.Client.Latency;
            var response = ResponseBuilder.Message(Context, $"Latency: {latency}ms");

            var sw = new Stopwatch();
            sw.Start();

            var message = await SendMessageAsync(response);

            sw.Stop();

            response = ResponseBuilder.Message(Context, $"Latency: {latency}ms\nPing: {sw.ElapsedMilliseconds}ms");

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
            var client = ClientFactory.CreateClient();

            using var response = await client.GetAsync("https://catfact.ninja/fact");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var obj = JObject.Parse(content);

                await SendOkAsync(0, obj["fact"]);
            }
            else
            {
                await SendNotOkAsync(1);
            }
        }

        [Command("Joke")]
        [RunMode(RunMode.Parallel)]
        [Name("Get Joke")]
        public async Task GetJokeAsync()
        {
            var client = ClientFactory.CreateClient();

            using var response = await client.GetAsync("https://icanhazdadjoke.com/");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var obj = JObject.Parse(content);

                await SendOkAsync(0, obj["joke"]);
            }
            else
            {
                await SendNotOkAsync(1);
            }
        }

        [Command("Gif")]
        [RunMode(RunMode.Parallel)]
        [Name("Get Gif")]
        public async Task GetGifAsync([Remainder] string search)
        {
            var client = ClientFactory.CreateClient();

            using var response = await client.GetAsync($"https://api.giphy.com/v1/gifs/random?api_key={Config.GiphyAPIKey}&rating=r&tag={search}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var obj = JObject.Parse(content);

                if (!obj["data"].Any())
                {
                    await SendNotOkAsync(0);
                }
                else
                {
                    var gifUrl = obj["data"]["image_original_url"];

                    using var stream = await client.GetStreamAsync($"{gifUrl}");

                    try
                    {
                        await SendFileAsync(stream, $"{search}.gif");
                    }
                    catch (HttpException ex) when (ex.DiscordCode == 40003)
                    {
                        await SendNotOkAsync(1);
                    }
                }
            }
            else
            {
                await SendNotOkAsync(2);
            }
        }


        [Command("Clear")]
        [RunMode(RunMode.Parallel)]
        [Name("Clear Messages")]
        public Task ClearMessagesAsync(int amount = 2)
            => Message.DeleteMessagesAsync(Context, amount);

        [Command("help")]
        [Name("Help Modules")]
        public async Task HelpAsync()
        {
            var modules = Services.GetService<CommandService>().GetAllModules();

            var canExecute = new List<Module>();

            foreach (var module in modules)
            {
                var result = await module.RunChecksAsync(Context, Services);

                if (result.IsSuccessful)
                {
                    var results = await module.Commands.Select(x => x.RunChecksAsync(Context, Services)).AllAsync();

                    if (results.Count(x => x.IsSuccessful) > 0)
                        canExecute.Add(module);
                }
            }

            var currentGuild = await Context.GuildStore.GetOrCreateGuildAsync(Context.Guild);
            var prefix = currentGuild.Prefixes.First();

            var builder = GetBuilder(prefix);

            builder.AddField("Modules", string.Join(", ", 
                canExecute.Select(x => $"`{Format.Sanitize(ulong.TryParse(x.Name, out _) ? Context.Guild.Name : x.Name)}`")));
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
            var currentGuild = await Context.GuildStore.GetOrCreateGuildAsync(Context.Guild);
            var prefix = currentGuild.Prefixes.First();

            var canExecute = new List<Command>();

            foreach (var command in module.Commands)
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
            var currentGuild = await Context.GuildStore.GetOrCreateGuildAsync(Context.Guild);
            var prefix = currentGuild.Prefixes.First();

            var builder = GetBuilder(prefix);

            builder.WithFooter("You can't go any deeper than this D:");

            foreach (var command in commands)
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
                Color = Utilities.EspeonColor,
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

        [Command("Mods")]
        [Name("List Mods")]
        public async Task ListModsAsync()
        {
            var currentGuild = await Context.GuildStore.GetOrCreateGuildAsync(Context.Guild);

            var modTasks = currentGuild.Moderators.Select(async x => Context.Guild.GetUser(x) as IGuildUser
                ?? await Context.Client.Rest.GetGuildUserAsync(Context.Guild.Id, x)).Where(x => !(x is null));

            var mods = await modTasks.AllAsync();

            if (mods.Length == 0)
            {
                await SendOkAsync(0);
            }
            else
            {
                await SendOkAsync(1,
                    string.Join(", ", mods.Select(x => $"{Format.Sanitize(x.GetDisplayName())}")));
            }
        }

        [Command("Admins")]
        [Name("List Admins")]
        public async Task ListAdminsAsync()
        {
            var currentGuild = await Context.GuildStore.GetOrCreateGuildAsync(Context.Guild);

            var adminTasks = currentGuild.Admins.Select(async x => Context.Guild.GetUser(x) as IGuildUser
                ?? await Context.Client.Rest.GetGuildUserAsync(Context.Guild.Id, x)).Where(x => !(x is null));

            var admins = await adminTasks.AllAsync();

            if (admins.Length == 0)
            {
                //should never happen but sure
                await SendOkAsync(0);
            }
            else
            {
                await SendOkAsync(1,
                    string.Join(", ", admins.Select(x => $"{Format.Sanitize(x.GetDisplayName())}")));
            }
        }
    }
}
