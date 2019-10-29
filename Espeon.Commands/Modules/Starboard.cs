using Disqord;
using Espeon.Core;
using Espeon.Core.Database;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Espeon.Commands {
	/*
	* Star
	* View
	* Stats
	* Random
	*/

	[Name("Starboard")]
	[Group("Star")]
	[Description("Display messages in a hall of fame")]
	public class Starboard : EspeonModuleBase {
		public Random Random { get; set; }

		[Command("enable")]
		[Name("Enable Starboard")]
		[RequireElevation(ElevationLevel.Admin)]
		[Description("Enables starboard settings the specified channel as the star channel")]
		public async Task EnableStarboardAsync([Remainder] CachedTextChannel channel) {
			Guild guild = Context.CurrentGuild;
			guild.StarboardChannelId = channel.Id;
			Context.GuildStore.Update(guild);

			await Task.WhenAll(Context.GuildStore.SaveChangesAsync(), SendOkAsync(0));
		}

		[Command("disable")]
		[Name("Disable Starboard")]
		[RequireElevation(ElevationLevel.Admin)]
		[Description("Disables starboard for this guild")]
		public async Task DisableStarboardAsync() {
			Guild guild = Context.CurrentGuild;
			guild.StarboardChannelId = 0;
			Context.GuildStore.Update(guild);

			await Task.WhenAll(Context.GuildStore.SaveChangesAsync(), SendOkAsync(0));
		}

		[Command("limit")]
		[Name("Set Starboard Limit")]
		[RequireElevation(ElevationLevel.Admin)]
		[Description("Change the number of stars needed for a message to be added to the starboard")]
		public async Task SetStarboardLimitAsync([RequireRange(1)] int limit) {
			Guild guild = Context.CurrentGuild;
			guild.StarLimit = limit;
			Context.GuildStore.Update(guild);

			await Task.WhenAll(Context.GuildStore.SaveChangesAsync(), SendOkAsync(0));
		}

		[Command("random")]
		[Name("Random Star")]
		[Description("Get a random message from the starboard")]
		public async Task ViewRandomStarAsync() {
			Guild guild = await Context.GuildStore.GetOrCreateGuildAsync(Context.Guild, x => x.StarredMessages);

			if (guild.StarredMessages.Count == 0) {
				await SendNotOkAsync(0);
				return;
			}

			StarredMessage randomStar = guild.StarredMessages[Random.Next(guild.StarredMessages.Count)];

			IUser user = await Context.Guild.GetOrFetchMemberAsync(randomStar.AuthorId) ??
			             await Context.Client.GetOrFetchUserAsync(randomStar.AuthorId);

			string jump = Core.Utilities.BuildJumpUrl(Context.Guild.Id, randomStar.ChannelId, randomStar.Id);

			LocalEmbed starMessage = Core.Utilities.BuildStarMessage(user, randomStar.Content, jump, randomStar.ImageUrl);

			string m = string.Concat($"{Core.Utilities.Star}", $"**{randomStar.ReactionUsers.Count}** - ",
				$"{(user as IMember)?.DisplayName ?? user.Name} in <#", $"{randomStar.ChannelId}>");

			await SendMessageAsync(m, starMessage);
		}
	}
}