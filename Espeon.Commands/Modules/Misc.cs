﻿using Disqord;
using Disqord.Rest;
using Espeon.Core;
using Espeon.Core.Database;
using Espeon.Core.Services;
using Kommon.Common;
using Microsoft.Extensions.DependencyInjection;
using MoreLinq;
using Newtonsoft.Json.Linq;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Espeon.Commands {
	/*
	* Cat Fact
	* Joke
	* Chuck
	* Gif
	* Clear Responses
	* Admins
	* Mods
	* Help
	* Emote
	*/

	[Name("Misc")]
	[Description("Commands that don't have their own home")]
	public class Misc : EspeonModuleBase {
		public Config Config { get; set; }
		public IEmoteService Emotes { get; set; }
		public IHttpClientFactory ClientFactory { get; set; }
		public TaskQueue Scheduler { get; set; }
		public ICommandHandlingService CommandHanlder { get; set; }

		private readonly LocalEmoji _delete = new LocalEmoji("🚮");

		[Command("Ping")]
		[Name("Ping")]
		[Description("View the bots gateway latency and REST ping")]
		[RunMode(RunMode.Parallel)]
		public async Task PingAsync() {
			User user = Context.Invoker;
			int latency = Client.Latency.GetValueOrDefault().Milliseconds;

			string resp = Responses.GetResponse(Context.Command.Module.Name, Context.Command.Name, user.ResponsePack, 0,
				latency);

			LocalEmbed response = ResponseBuilder.Message(Context, resp);

			var sw = new Stopwatch();
			sw.Start();

			IUserMessage message = await SendMessageAsync(response);

			sw.Stop();

			resp = Responses.GetResponse(Context.Command.Module.Name, Context.Command.Name, user.ResponsePack, 1,
				latency, sw.ElapsedMilliseconds);

			response = ResponseBuilder.Message(Context, resp);

			await message.ModifyAsync(x => x.Embed = response);
		}


		[Command("Catfact")]
		[RunMode(RunMode.Parallel)]
		[Name("Get Catfact")]
		[Cooldown(1, 1, CooldownMeasure.Minutes, CooldownBucket.API)]
		[Description("Gets a random cat fact")]
		public async Task GetCatfactAsync() {
			HttpClient client = ClientFactory.CreateClient();

			using HttpResponseMessage response = await client.GetAsync("https://catfact.ninja/fact");

			if (response.IsSuccessStatusCode) {
				string content = await response.Content.ReadAsStringAsync();
				JObject obj = JObject.Parse(content);

				await SendOkAsync(0, obj["fact"]);
			} else {
				await SendNotOkAsync(1);
			}
		}

		[Command("Joke")]
		[RunMode(RunMode.Parallel)]
		[Name("Get Joke")]
		[Cooldown(1, 1, CooldownMeasure.Minutes, CooldownBucket.API)]
		[Description("Gets a random joke")]
		public async Task GetJokeAsync() {
			HttpClient client = ClientFactory.CreateClient();

			using HttpResponseMessage response = await client.GetAsync("https://icanhazdadjoke.com/");

			if (response.IsSuccessStatusCode) {
				string content = await response.Content.ReadAsStringAsync();
				JObject obj = JObject.Parse(content);

				await SendOkAsync(0, obj["joke"]);
			} else {
				await SendNotOkAsync(1);
			}
		}

		[Command("Gif")]
		[RunMode(RunMode.Parallel)]
		[Name("Get Gif")]
		[Cooldown(1, 1, CooldownMeasure.Minutes, CooldownBucket.API)]
		[Description("Gets a gif based on the specified search")]
		public async Task GetGifAsync([Remainder] string search) {
			HttpClient client = ClientFactory.CreateClient();

			using HttpResponseMessage response =
				await client.GetAsync(
					$"https://api.giphy.com/v1/gifs/random?api_key={Config.GiphyAPIKey}&rating=r&tag={search}");

			if (response.IsSuccessStatusCode) {
				string content = await response.Content.ReadAsStringAsync();
				JObject obj = JObject.Parse(content);

				if (!obj["data"].Any()) {
					await SendNotOkAsync(0);
				} else {
					JToken gifUrl = obj["data"]["image_original_url"];

					using Stream stream = await client.GetStreamAsync($"{gifUrl}");

					try {
						await SendFileAsync(stream, $"{search}.gif");
					} catch (DiscordHttpException ex) {
						await SendNotOkAsync(1);
					}
				}
			} else {
				await SendNotOkAsync(2);
			}
		}


		[Command("Clear")]
		[RunMode(RunMode.Parallel)]
		[Name("Clear Messages")]
		[Description("Clears the bots responses")]
		public Task ClearMessagesAsync(int amount = 5) {
			return Message.DeleteMessagesAsync(Channel, Guild.CurrentMember, Member.Id, amount);
		}

		[Command("help")]
		[Name("Help Modules")]
		[Description("View generic help on the bot")]
		public async Task HelpAsync() {
			IReadOnlyList<Module> modules = Services.GetService<CommandService>().GetAllModules();

			var canExecute = new List<Module>();

			foreach (Module module in modules) {
				IResult result = await module.RunChecksAsync(Context);

				if (!result.IsSuccessful) {
					continue;
				}

				IResult[] results = await module.Commands.Select(x => x.RunChecksAsync(Context)).AllAsync();

				if (results.Count(x => x.IsSuccessful) > 0) {
					canExecute.Add(module);
				}
			}

			string prefix = Context.PrefixUsed;

			LocalEmbedBuilder builder = GetBuilder()
				.AddField("Modules",
					string.Join(", ",
						canExecute.Select(x =>
							$"`{Markdown.Escape(ulong.TryParse(x.Name, out _) ? Context.Guild.Name : x.Name)}`")))
				.WithFooter($"To view help with a specific module invoke {prefix}help Module").WithDescription(
					$"Hello, my name is Espeon.Core{Emotes["Espeon"]}! " +
					$"You can invoke my commands either by mentioning me or using the `{Markdown.Escape(prefix)}` prefix!");

			IUserMessage message = await SendMessageAsync(builder.Build());

			var deleteCallback = new DeleteCallback(Context, message, this._delete,
				new ReactionFromSourceUser(Member.Id));

			await TryAddCallbackAsync(deleteCallback);
		}

		[Command("help")]
		[Name("Help Module")]
		[Priority(0)]
		[Description("View help on the specified module")]
		public async Task HelpAsync([Remainder] Module module) {
			string prefix = Context.PrefixUsed;

			var canExecute = new List<Command>();

			foreach (Command command in module.Commands) {
				IResult result = await command.RunChecksAsync(Context);

				if (result.IsSuccessful) {
					canExecute.Add(command);
				}
			}

			LocalEmbedBuilder builder = GetBuilder()
				.AddField(ulong.TryParse(module.Name, out _) ? Guild.Name : module.Name ?? "\u200b",
					module.Description ?? "\u200b")
				.AddField("Commands",
					string.Join(", ", canExecute.Select(x => $"`{Markdown.Escape(x.FullAliases.First().ToLower())}`")))
				.WithFooter($"To view help with a specific command invoke {prefix}help Command Name");

			IUserMessage message = await SendMessageAsync(builder.Build());

			var deleteCallback = new DeleteCallback(Context, message, this._delete,
				new ReactionFromSourceUser(Member.Id));

			await TryAddCallbackAsync(deleteCallback);
		}

		[Command("help")]
		[Name("Help Commands")]
		[Priority(1)]
		[Description("View help on the specified command(s)")]
		public async Task HelpAsync([Remainder] IReadOnlyCollection<Command> commands) {
			Command[][] batch = commands.Batch(3).Select(x => x.ToArray()).ToArray();
			var toSend = new List<LocalEmbedBuilder>();

			string prefix = Context.PrefixUsed;

			string GetParameters(Command command) {
				var sb = new StringBuilder();

				foreach (Parameter param in command.Parameters) {
					sb.Append(Utilities.ExampleUsage.TryGetValue(param.Type, out string usage)
						? $" `{usage}`"
						: $" `{param.Name}`");
				}

				return sb.ToString();
			}

			if (batch.Length == 1 && batch[0].Length == 1) {
				Command cmd = batch[0][0];

				LocalEmbedBuilder builder = GetBuilder()
					.AddField(cmd.Name, prefix + cmd.FullAliases.First().ToLower() + GetParameters(cmd))
					.AddField("Summary", cmd.Description ?? "\u200b").AddField("Aliases",
						string.Join(", ", cmd.FullAliases.Select(x => x.ToLower())) ?? "\u200b");

				IUserMessage message = await SendMessageAsync(builder.Build());

				var deleteCallback = new DeleteCallback(Context, message, this._delete,
					new ReactionFromSourceUser(Member.Id));

				await TryAddCallbackAsync(deleteCallback);

				return;
			}

			foreach (Command[] col in batch) {
				LocalEmbedBuilder builder = GetBuilder()
					.WithFooter($"Type {prefix}help Command Name to view help for that specific command");

				foreach (Command command in col) {
					//TODO add summaries/examples
					builder.AddField(command.Name,
						$"{prefix}{command.FullAliases.First().ToLower()}{GetParameters(command)}");
				}

				toSend.Add(builder);
			}

			if (toSend.Count == 1) {
				IUserMessage message = await SendMessageAsync(toSend[0].Build());

				var deleteCallback = new DeleteCallback(Context, message, this._delete,
					new ReactionFromSourceUser(Member.Id));

				await TryAddCallbackAsync(deleteCallback);

				return;
			}

			var i = 0;
			PaginatorOptions options = PaginatorOptions.Default(toSend.ToDictionary(_ => i++,
				x => (string.Empty, x.WithFooter($"Page {i}/{toSend.Count}").Build())));

			var paginator = new DefaultPaginator(Context, Interactive, Message, options,
				new ReactionFromSourceUser(Member.Id.RawValue));

			await TryAddCallbackAsync(paginator);
		}

		private LocalEmbedBuilder GetBuilder() {
			return new LocalEmbedBuilder {
				Color = Core.Utilities.EspeonColor,
				Title = "Espeon.Core's Help",
				Author = new LocalEmbedAuthorBuilder() {
					IconUrl = Context.Member.GetAvatarUrl(),
					Name = Context.Member.DisplayName
				},
				ThumbnailUrl = Context.Guild.CurrentMember.GetAvatarUrl(),
				Timestamp = DateTimeOffset.UtcNow
			};
		}

		[Command("Mods")]
		[Name("List Mods")]
		[Description("List the guilds bot moderators")]
		public async Task ListModsAsync() {
			Guild currentGuild = Context.CurrentGuild;

			IEnumerable<Task<IMember>> modTasks = currentGuild.Moderators.Select(async x =>
					Guild.GetMember(x) as IMember ?? await Client.GetMemberAsync(Guild.Id, x == 0 ? 1 : x))
				.Where(x => !(x is null));

			IMember[] mods = await modTasks.AllAsync();

			if (mods.Length == 0) {
				await SendOkAsync(0);
			} else {
				await SendOkAsync(1, string.Join(", ", mods.Select(x => $"{Markdown.Escape(x.DisplayName)}")));
			}
		}

		[Command("Admins")]
		[Name("List Admins")]
		[Description("List the guilds bot admins")]
		public async Task ListAdminsAsync() {
			Guild currentGuild = Context.CurrentGuild;

			IEnumerable<Task<IMember>> adminTasks = currentGuild.Admins.Select(
					async x => Guild.GetMember(x) as IMember ?? await Client.GetMemberAsync(Guild.Id, x))
				.Where(x => !(x is null));

			IMember[] admins = await adminTasks.AllAsync();

			if (admins.Length == 0) {
				//should never happen but sure
				await SendOkAsync(0);
			} else {
				await SendOkAsync(1, string.Join(", ", admins.Select(x => $"{Markdown.Escape(x.DisplayName)}")));
			}
		}

		[Command("Emote")]
		[Name("Steal Emote")]
		[RequirePermissions(PermissionTarget.Bot, PermissionType.Guild, Permission.ManageEmojis)]
		[RequirePermissions(PermissionTarget.User, PermissionType.Guild, Permission.ManageEmojis)]
		[Description("Adds the specified emote(s) to your guild")]
		public async Task StealEmoteAsync(params LocalCustomEmoji[] emojis) {
			int animatedCount = Guild.Emojis.Count(x => x.Value.IsAnimated);
			int normalCount = Guild.Emojis.Count(x => !x.Value.IsAnimated);

			var failed = new List<LocalCustomEmoji>();
			HttpClient client = ClientFactory.CreateClient();

			var added = 0;

			foreach (LocalCustomEmoji emoji in emojis) {
				if (emoji.IsAnimated && animatedCount >= 50) {
					failed.Add(emoji);
					continue;
				}

				if (!emoji.IsAnimated && normalCount >= 50) {
					failed.Add(emoji);
					continue;
				}

				Stream stream = await client.GetStreamAsync(emoji.GetUrl());
				await Guild.CreateEmojiAsync(emoji.Name, new LocalAttachment(stream, emoji.Name),
					options: RestRequestOptions.FromReason("Emote stolen"));

				animatedCount = Guild.Emojis.Count(x => x.Value.IsAnimated);
				normalCount = Guild.Emojis.Count(x => !x.Value.IsAnimated);

				added++;

				stream.Dispose();
			}

			if (failed.Count < emojis.Length) {
				await SendOkAsync(0, added);
			}

			if (failed.Count > 0) {
				await SendNotOkAsync(1, string.Join(", ", failed.Select(x => x.Name)));
			}
		}

		[Command("Quote")]
		[Name("Quote Message")]
		[Description("Quotes the message corresponding to the passed id")]
		public async Task QuoteMessageAsync(ulong id) {
			LocalEmbed res = await Core.Utilities.QuoteFromMessageIdAsync(Channel, id);

			if (res is null) {
				await SendNotOkAsync(0);
			} else {
				await SendMessageAsync(res);
			}
		}

		[Command("Quote")]
		[Name("Quote Link")]
		[Description("Quotes the specified jump link")]
		public async Task QuoteLinkAsync(string jumpUrl) {
			LocalEmbed res = await Core.Utilities.QuoteFromStringAsync(Client, jumpUrl);

			if (res is null) {
				await SendNotOkAsync(0);
			} else {
				await SendMessageAsync(res);
			}
		}

		[Command("Quote")]
		[Name("Last Quote")]
		[Description("Quotes the last jump link sent in the channel")]
		public async Task QuoteMessageAsync() {
			bool found = Services.GetService<IQuoteService>().TryGetLastJumpMessage(Channel.Id, out ulong messageId);

			if (!found) {
				await SendNotOkAsync(0);
				return;
			}

			IMessage message = await Channel.GetMessageAsync(messageId);

			if (message is null) {
				await SendNotOkAsync(1);
				return;
			}

			LocalEmbed res = await Core.Utilities.QuoteFromStringAsync(Client, message.Content);

			if (res is null) {
				await SendNotOkAsync(2);
			} else {
				await SendMessageAsync(res);
			}
		}

		[Command("Inspect")]
		[Name("Inspect")]
		[Description("Inspect Discord related entities")]
		public async Task InspectAsync(string input) {
			string[] split = input.Split(':', StringSplitOptions.RemoveEmptyEntries);

			if (split.Length > 2) {
				await SendMessageAsync("Failed to parse input");
				return;
			}

			string target = split[0].ToLower();
			ulong id = 0;

			if (split.Length > 1 && !ulong.TryParse(split[1], out id)) {
				await SendMessageAsync("Failed to parse id");
				return;
			}

			object toInspect = target switch {
				"user" => id == 0
					? Member
					: await Guild.GetOrFetchMemberAsync(id) ?? await Client.GetOrFetchUserAsync(id),
				"channel" => id == 0 ? Channel : Client.GetChannel(id),
				"role"    => Guild.GetRole(id),
				"guild"   => id == 0 ? Guild : Client.GetGuild(id),
				"message" => id == 0 ? Context.Message as IMessage : await Context.Channel.GetMessageAsync(id),
				_         => null
			};

			if (toInspect is null) {
				await SendMessageAsync("Failed to find entity with given id");
				return;
			}

			foreach (string message in toInspect.Inspect()) {
				await SendMessageAsync($"```css\n{message}\n```");
			}
		}

		[Command("Mock")]
		[Name("Mock Message")]
		[RequirePermissions(PermissionTarget.Bot, PermissionType.Channel, Permission.ManageWebhooks)]
		[Description("Mock the specified message")]
		public async Task MockAsync(ulong messageId) {
			IMessage message = await Context.Channel.GetMessageAsync(messageId);

			if (message is null) {
				await SendNotOkAsync(0);
				return;
			}

			Guild guild = await Context.GuildStore.GetOrCreateGuildAsync(Context.Guild, x => x.Webhooks);

			MockWebhook foundWebhook = guild.Webhooks.Find(x => x.ChannelId == Context.Channel.Id);
			IReadOnlyCollection<RestWebhook> webhooks = await Channel.GetWebhooksAsync();
			RestWebhook wh = webhooks.FirstOrDefault(x => x.Id == foundWebhook?.Id);

			if (wh is null) {
				guild.Webhooks?.RemoveAll(x => x?.Id == foundWebhook?.Id);

				Context.GuildStore.Update(guild);
				await Context.GuildStore.SaveChangesAsync();

				//hack
				foundWebhook = null;
			}

			if (foundWebhook is null) {
				wh = await Context.Channel.CreateWebhookAsync("Espeon Webhook");

				foundWebhook = new MockWebhook {
					Guild = guild,
					GuildId = guild.Id,
					Id = wh.Id,
					Token = wh.Token,
					ChannelId = Context.Channel.Id
				};

				guild.Webhooks.Add(foundWebhook);

				Context.GuildStore.Update(guild);
				await Context.GuildStore.SaveChangesAsync();
			}

			using var whc = new RestWebhookClient(wh);

			static string Mockify(string content) {
				var index = 0;

				return string.Concat(content.Select(x => index++ % 2 == 0
					? x.ToString().ToUpper()
					: x.ToString().ToLower()));
			}

			await whc.ExecuteAsync(Mockify(message.Content),
				name: (await Guild.GetOrFetchMemberAsync(message.Author.Id))?.DisplayName,
				avatarUrl: message.Author.GetAvatarUrl(), wait: true);
		}

		[Command("delayed")]
		[Name("Delayed Execution")]
		public Task DelayedExecutionAsync(TimeSpan executeIn, [Remainder] string command) {
			Scheduler.ScheduleTask(executeIn,
				() => CommandHanlder.ExecuteCommandAsync(Member, Channel, Context.PrefixUsed + command,
					Context.Message));

			return SendOkAsync(0);
		}
	}
}