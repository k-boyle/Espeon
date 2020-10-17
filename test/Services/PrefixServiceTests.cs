using Disqord;
using Disqord.Bot.Prefixes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Espeon.Test {
    public class PrefixServiceTests {
        private static readonly ILogger<PrefixService> Logger = new NullLogger<PrefixService>();
        
        private static readonly Snowflake GuildId = 0L;
        private static readonly TestGuild Guild = new TestGuild("PrefixServiceTests", GuildId);
        
        private GuildPrefixes _guildPrefixes;
        private IServiceProvider _provider;

        [SetUp]
        public async Task BeforeEachAsync() {
            this._guildPrefixes = new GuildPrefixes(GuildId);
            this._provider = new ServiceCollection()
                .AddSingleton(Logger)
                .AddDbContext<EspeonDbContext>(builder => builder.UseInMemoryDatabase("espeon"))
                .BuildServiceProvider();
            
            using var scope = this._provider.CreateScope();
            await using var context = scope.ServiceProvider.GetRequiredService<EspeonDbContext>();

            await context.GuildPrefixes.AddAsync(this._guildPrefixes);
            await context.SaveChangesAsync();
        }
        
        [TearDown]
        public async Task TearDownAsync() {
            using var scope = this._provider.CreateScope();
            await using var context = scope.ServiceProvider.GetRequiredService<EspeonDbContext>();
            await context.Database.EnsureDeletedAsync();
        }
        
        [Test]
        public async Task TestGetPrefixsAsync() {
            var service = new PrefixService(this._provider, Logger);
            var prefixes = await service.GetPrefixesAsync(Guild);
            CollectionAssert.AreEquivalent(new IPrefix[] { MentionPrefix.Instance, new StringPrefix("es/") }, prefixes);
        }
        
        [Test]
        public async Task TestAlreadyAddedPrefixAsync() {
            var service = new PrefixService(this._provider, Logger, new ConcurrentDictionary<ulong, GuildPrefixes> {
                [GuildId] = this._guildPrefixes
            });
            var result = await service.TryAddPrefixAsync(Guild, MentionPrefix.Instance);
            Assert.False(result);
        }
        
        [Test]
        public async Task TestNotAddedPrefixAsync() {
            var service = new PrefixService(this._provider, Logger, new ConcurrentDictionary<ulong, GuildPrefixes> {
                [GuildId] = this._guildPrefixes
            });
            var result = await service.TryAddPrefixAsync(Guild, new StringPrefix("kieran is cool"));
            Assert.True(result);
        }
        
        [Test]
        public async Task TestRemoveAddedPrefixAsync() {
            var service = new PrefixService(this._provider, Logger, new ConcurrentDictionary<ulong, GuildPrefixes> {
                [GuildId] = this._guildPrefixes
            });
            var result = await service.TryRemovePrefixAsync(Guild, MentionPrefix.Instance);
            Assert.True(result);
        }
        
        [Test]
        public async Task TestRemoveNotAddedPrefixAsync() {
            var service = new PrefixService(this._provider, Logger, new ConcurrentDictionary<ulong, GuildPrefixes> {
                [GuildId] = this._guildPrefixes
            });
            var result = await service.TryRemovePrefixAsync(Guild, new StringPrefix("kieran is cool"));
            Assert.False(result);
        }
    }
}