using Disqord.Bot.Prefixes;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace Espeon.Test {
    public class EspeonDbContextTests {
        private const ulong GuildId = 0;
        private const ulong UserId = 0;

        private DbContextOptions _options;
        
        [SetUp]
        public async Task BeforeEachAsync() {
            this._options = new DbContextOptionsBuilder()
                .UseInMemoryDatabase("espeon")
                .Options;
            
            await using var context = new EspeonDbContext(this._options);

            await context.UserLocalisations.AddAsync(new UserLocalisation(GuildId, UserId) {
                Value = Language.Owo
            });
            await context.GuildPrefixes.AddAsync(new GuildPrefixes(GuildId));
            await context.GuildTags.AddAsync(new GuildTags(GuildId));
            await context.SaveChangesAsync();
        }
        
        [TearDown]
        public async Task AfterEachAsync() {
            await using var context = new EspeonDbContext(this._options);
            await context.Database.EnsureDeletedAsync();
        }
        
        [Test]
        public async Task TestGetOrCreate2IdWhenInDbAsync() {
            await using var context = new EspeonDbContext(this._options);
            var local = await context.GetOrCreateAsync<UserLocalisation, ulong>(
                GuildId,
                UserId,
                (guildId, userId) => throw new InvalidOperationException("Entity should be in db"));
            
            Assert.AreEqual(GuildId, local.GuildId);
            Assert.AreEqual(UserId, local.UserId);
            Assert.AreEqual(Language.Owo, local.Value);
        }
        
        [Test]
        public async Task TestGetOrCreate2IdWhenNotInDbAsync() {
            const ulong guildId = 1;
            const ulong userId = 1;
            
            await using var context = new EspeonDbContext(this._options);
            var local = await context.GetOrCreateAsync(
                guildId,
                userId,
                (guildId, userId) => new UserLocalisation(guildId, userId));
            
            Assert.AreEqual(guildId, local.GuildId);
            Assert.AreEqual(userId, local.UserId);
            Assert.AreEqual(Language.Default, local.Value);
        }

        [Test]
        public async Task TestGetOrCreate1IdWhenInDbAsync() {
            await using var context = new EspeonDbContext(this._options);
            var prefixes = await context.GetOrCreateAsync<GuildPrefixes, ulong>(
                GuildId,
                guildId => throw new InvalidOperationException("Entity should be in db"));

            //this should really test against a different set to the default
            //but there's a bug with inmemory database that means even though
            //we save a modified one, we will get the default one when we construct
            //a new context
            Assert.AreEqual(GuildId, prefixes.GuildId);
            CollectionAssert.AreEquivalent(
                new IPrefix[] {
                    MentionPrefix.Instance,
                    new StringPrefix("es/")
                },
                prefixes.Values);
        }

        [Test]
        public async Task TestGetOrCreate1IdWhenNotInDbAsync() {
            const ulong guildId = 1;

            await using var context = new EspeonDbContext(this._options);
            var prefixes = await context.GetOrCreateAsync(
                guildId,
                guildId => new GuildPrefixes(guildId));

            Assert.AreEqual(guildId, prefixes.GuildId);
            CollectionAssert.AreEquivalent(
                new IPrefix[] {
                    MentionPrefix.Instance,
                    new StringPrefix("es/")
                },
                prefixes.Values);
        }
        
        [Test]
        public async Task TestFindAndIncludeAsync() {
            await using var context = new EspeonDbContext(this._options);
            var tags = await context.IncludeAndFindAsync<GuildTags, GuildTag, ulong>(
                GuildId,
                tags => tags.Values);
            
            Assert.NotNull(tags);
            Assert.NotNull(tags.Values);
        }
        
        [Test]
        public async Task TestUpdateAsync() {
            await using var context = new EspeonDbContext(this._options);
            var tags = await context.IncludeAndFindAsync<GuildTags, GuildTag, ulong>( 
                GuildId,
                tags => tags.Values);
            tags.Values.Add(new GuildTag(GuildId, "espeon", "tag", UserId));
            await context.UpdateAsync(tags);
            var tags2 = await context.IncludeAndFindAsync<GuildTags, GuildTag, ulong>(
                GuildId,
                tags => tags.Values);
            CollectionAssert.AreEquivalent(tags.Values, tags2.Values);
        }
        
        [Test]
        public async Task TestPersistAsync() {
            const ulong guildId = 1;
            
            await using var context = new EspeonDbContext(this._options);
            await context.PersistAsync(new GuildPrefixes(guildId));
            var found = await context.GuildPrefixes.FindAsync(guildId);
            Assert.NotNull(found);
        }
        
        [Test]
        public async Task TestRemoveAsync() {
            await using var context = new EspeonDbContext(this._options);
            var prefixes = await context.GuildPrefixes.FindAsync(GuildId);
            await context.RemoveAsync(prefixes);
            prefixes = await context.GuildPrefixes.FindAsync(GuildId);
            Assert.Null(prefixes);
        }
    }
}