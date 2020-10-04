using Disqord;
using Disqord.Rest;
using Disqord.Rest.AuditLogs;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace Espeon.Test {
    public class TestGuild : IGuild {
        public string Name { get; }
        public Snowflake Id { get; }

        public TestGuild(string name, Snowflake id) {
            Name = name;
            Id = id;
        }

        public IRestDiscordClient Client { get; }

        public DateTimeOffset CreatedAt { get; }

        public string IconHash { get; }

        public string SplashHash { get; }

        public string DiscoverySplashHash { get; }

        public Snowflake OwnerId { get; }

        public string VoiceRegionId { get; }

        public Snowflake? AfkChannelId { get; }

        public int AfkTimeout { get; }

        public Snowflake? EmbedChannelId { get; }

        public bool IsEmbedEnabled { get; }

        public VerificationLevel VerificationLevel { get; }

        public DefaultNotificationLevel DefaultNotificationLevel { get; }

        public ContentFilterLevel ContentFilterLevel { get; }

        public IReadOnlyDictionary<Snowflake, IRole> Roles { get; }

        public IReadOnlyDictionary<Snowflake, IGuildEmoji> Emojis { get; }

        public IReadOnlyList<string> Features { get; }

        public MfaLevel MfaLevel { get; }

        public Snowflake? ApplicationId { get; }

        public bool IsWidgetEnabled { get; }

        public Snowflake? WidgetChannelId { get; }

        public Snowflake? SystemChannelId { get; }

        public int MaxPresenceCount { get; }

        public int MaxMemberCount { get; }

        public string VanityUrlCode { get; }

        public string Description { get; }

        public string BannerHash { get; }

        public BoostTier BoostTier { get; }

        public int BoostingMemberCount { get; }

        public CultureInfo PreferredLocale { get; }

        public Task DeleteAsync(RestRequestOptions options = null) {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<RestWebhook>> GetWebhooksAsync(RestRequestOptions options = null) {
            throw new NotImplementedException();
        }

        public RestRequestEnumerable<RestAuditLog> GetAuditLogsEnumerable(
            int limit = 100,
            Snowflake? userId = null,
            Snowflake? startFromId = null,
            RestRequestOptions options = null) {
            throw new NotImplementedException();
        }

        public RestRequestEnumerable<T> GetAuditLogsEnumerable<T>(
            int limit = 100,
            Snowflake? userId = null,
            Snowflake? startFromId = null,
            RestRequestOptions options = null) where T : RestAuditLog {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<RestAuditLog>> GetAuditLogsAsync(
            int limit = 100,
            Snowflake? userId = null,
            Snowflake? startFromId = null,
            RestRequestOptions options = null) {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<T>> GetAuditLogsAsync<T>(
            int limit = 100,
            Snowflake? userId = null,
            Snowflake? startFromId = null,
            RestRequestOptions options = null) where T : RestAuditLog {
            throw new NotImplementedException();
        }

        public Task ModifyAsync(Action<ModifyGuildProperties> action, RestRequestOptions options = null) {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<RestGuildChannel>> GetChannelsAsync(RestRequestOptions options = null) {
            throw new NotImplementedException();
        }

        public Task<RestTextChannel> CreateTextChannelAsync(string name, Action<CreateTextChannelProperties> action = null, RestRequestOptions options = null) {
            throw new NotImplementedException();
        }

        public Task<RestVoiceChannel> CreateVoiceChannelAsync(string name, Action<CreateVoiceChannelProperties> action = null, RestRequestOptions options = null) {
            throw new NotImplementedException();
        }

        public Task<RestCategoryChannel> CreateCategoryChannelAsync(string name, Action<CreateCategoryChannelProperties> action = null, RestRequestOptions options = null) {
            throw new NotImplementedException();
        }

        public Task ReorderChannelsAsync(IReadOnlyDictionary<Snowflake, int> positions, RestRequestOptions options = null) {
            throw new NotImplementedException();
        }

        public Task<RestMember> GetMemberAsync(Snowflake memberId, RestRequestOptions options = null) {
            throw new NotImplementedException();
        }

        public RestRequestEnumerable<RestMember> GetMembersEnumerable(int limit, Snowflake? startFromId = null, RestRequestOptions options = null) {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<RestMember>> GetMembersAsync(int limit = 1000, Snowflake? startFromId = null, RestRequestOptions options = null) {
            throw new NotImplementedException();
        }

        public Task ModifyMemberAsync(Snowflake memberId, Action<ModifyMemberProperties> action, RestRequestOptions options = null) {
            throw new NotImplementedException();
        }

        public Task ModifyOwnNickAsync(string nick, RestRequestOptions options = null) {
            throw new NotImplementedException();
        }

        public Task GrantRoleAsync(Snowflake memberId, Snowflake roleId, RestRequestOptions options = null) {
            throw new NotImplementedException();
        }

        public Task RevokeRoleAsync(Snowflake memberId, Snowflake roleId, RestRequestOptions options = null) {
            throw new NotImplementedException();
        }

        public Task KickMemberAsync(Snowflake memberId, RestRequestOptions options = null) {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<RestBan>> GetBansAsync(RestRequestOptions options = null) {
            throw new NotImplementedException();
        }

        public Task<RestBan> GetBanAsync(Snowflake userId, RestRequestOptions options = null) {
            throw new NotImplementedException();
        }

        public Task BanMemberAsync(
            Snowflake memberId,
            string reason = null,
            int? deleteMessageDays = null,
            RestRequestOptions options = null) {
            throw new NotImplementedException();
        }

        public Task UnbanMemberAsync(Snowflake userId, RestRequestOptions options = null) {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<RestRole>> GetRolesAsync(RestRequestOptions options = null) {
            throw new NotImplementedException();
        }

        public Task<RestRole> CreateRoleAsync(Action<CreateRoleProperties> action = null, RestRequestOptions options = null) {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<RestRole>> ReorderRolesAsync(IReadOnlyDictionary<Snowflake, int> positions, RestRequestOptions options = null) {
            throw new NotImplementedException();
        }

        public Task<RestRole> ModifyRoleAsync(Snowflake roleId, Action<ModifyRoleProperties> action, RestRequestOptions options = null) {
            throw new NotImplementedException();
        }

        public Task DeleteRoleAsync(Snowflake roleId, RestRequestOptions options = null) {
            throw new NotImplementedException();
        }

        public Task<int> GetPruneCountAsync(int days, RestRequestOptions options = null) {
            throw new NotImplementedException();
        }

        public Task<int?> PruneAsync(int days, bool computePruneCount = true, RestRequestOptions options = null) {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<RestGuildVoiceRegion>> GetVoiceRegionsAsync(RestRequestOptions options = null) {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<RestInvite>> GetInvitesAsync(RestRequestOptions options = null) {
            throw new NotImplementedException();
        }

        public Task<RestWidget> GetWidgetAsync(RestRequestOptions options = null) {
            throw new NotImplementedException();
        }

        public Task<RestWidget> ModifyWidgetAsync(Action<ModifyWidgetProperties> action, RestRequestOptions options = null) {
            throw new NotImplementedException();
        }

        public Task<string> GetVanityInviteAsync(RestRequestOptions options = null) {
            throw new NotImplementedException();
        }

        public Task LeaveAsync(RestRequestOptions options = null) {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<RestGuildEmoji>> GetEmojisAsync(RestRequestOptions options = null) {
            throw new NotImplementedException();
        }

        public Task<RestGuildEmoji> GetEmojiAsync(Snowflake emojiId, RestRequestOptions options = null) {
            throw new NotImplementedException();
        }

        public Task<RestGuildEmoji> CreateEmojiAsync(Stream image, string name, IEnumerable<Snowflake> roleIds = null, RestRequestOptions options = null) {
            throw new NotImplementedException();
        }

        public Task<RestGuildEmoji> ModifyEmojiAsync(Snowflake emojiId, Action<ModifyGuildEmojiProperties> action, RestRequestOptions options = null) {
            throw new NotImplementedException();
        }

        public Task DeleteEmojiAsync(Snowflake emojiId, RestRequestOptions options = null) {
            throw new NotImplementedException();
        }

        public string GetIconUrl(ImageFormat format = ImageFormat.Default, int size = 2048) {
            throw new NotImplementedException();
        }

        public string GetSplashUrl(int size = 2048) {
            throw new NotImplementedException();
        }

        public string GetDiscoverySplashUrl(int size = 2048) {
            throw new NotImplementedException();
        }

        public string GetBannerUrl(int size = 2048) {
            throw new NotImplementedException();
        }
    }
}