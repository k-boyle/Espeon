using Disqord;
using Disqord.Rest;
using System.Threading.Tasks;

namespace Espeon.Core {
	public static partial class Extensions {
		private const string JUMP_BASE_URL = "https://discordapp.com/channels/";

		public static async ValueTask<string> GetJumpUrlAsync(this IMessage message) {
			return message switch {
				CachedMessage cached => cached.JumpUrl,
				RestMessage rest => await FromRestAsync(rest),
				_                    => ""
			};
		}

		private static async Task<string> FromRestAsync(RestMessage rest) {
			RestDiscordClient client = rest.Client;
			IChannel channel = await client.GetChannelAsync(rest.ChannelId);

			return channel switch {
				IDmChannel dm     => $"{JUMP_BASE_URL}@me/{channel.Id}/{rest.Id}",
				ITextChannel text => $"{JUMP_BASE_URL}{text.GuildId}/{channel.Id}/{rest.Id}",
				_                 => ""
			};
		}

		public static async ValueTask<IMember> GetOrFetchMemberAsync(this IGuild guild, ulong memberId) {
			return guild switch {
				CachedGuild cached => cached.GetMember(memberId) as IMember ?? await cached.GetMemberAsync(memberId),
				RestGuild rest     => await rest.GetMemberAsync(memberId),
				_                  => null
			};
		}

		public static async ValueTask<IUser> GetOrFetchUserAsync(this DiscordClient client, ulong userId) {
			return client.GetUser(userId) as IUser ?? await client.GetUserAsync(userId);
		}

		public static async ValueTask<T> GetOrDownloadAsync<T, TCache, TRest>(
			this DownloadableOptionalSnowflakeEntity<TCache, TRest> downloadable)
			where TCache : CachedSnowflakeEntity, T where TRest : RestSnowflakeEntity, T {
			return downloadable.HasValue
				? (T) downloadable.Value
				: await downloadable.Downloadable.GetOrDownloadAsync();
		}
	}
}