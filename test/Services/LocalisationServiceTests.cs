using Disqord;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Espeon.Test {
    public class LocalisationServiceTests {
        private static readonly ILogger<LocalisationService> Logger = new NullLogger<LocalisationService>();
        
        private static readonly Snowflake Member1Id = 0L;
        private static readonly Snowflake Member2Id = 1L;
        private static readonly Snowflake Member3Id = 2L;
        private static readonly Snowflake GuildId = 0L;
        private static readonly ILocalisationProvider LocalisationProvider = new TestLocalisationProvider();
        
        private IServiceProvider _provider;
        
        [SetUp]
        public async Task BeforeEachAsync() {
            this._provider = new ServiceCollection()
                .AddSingleton(Logger)
                .AddDbContext<EspeonDbContext>(builder => builder.UseInMemoryDatabase("espeon"))
                .BuildServiceProvider();
            
            using var scope = this._provider.CreateScope();
            await using var context = scope.ServiceProvider.GetRequiredService<EspeonDbContext>();

            await context.UserLocalisations.AddAsync(new UserLocalisation(Member1Id, GuildId));
            await context.UserLocalisations.AddAsync(new UserLocalisation(Member3Id, GuildId) {
                Value = Language.Owo
            });
            await context.SaveChangesAsync();
        }
        
        [TearDown]
        public async Task TearDownAsync() {
            using var scope = this._provider.CreateScope();
            await using var context = scope.ServiceProvider.GetRequiredService<EspeonDbContext>();
            await context.Database.EnsureDeletedAsync();
        }
        
        [Test]
        public async Task TestGetResponseForLocalisationInDbAsync() {
            var service = new LocalisationService(this._provider, LocalisationProvider, Logger);
            await service.StartAsync(CancellationToken.None);

            var response = await service.GetResponseAsync(Member1Id, GuildId, LocalisationStringKey.PING_COMMAND);
            Assert.False(string.IsNullOrWhiteSpace(response));
        }
        
        [Test]
        public async Task TestGetResponseForLocalisationNotInDbAsync() {
            var service = new LocalisationService(this._provider, LocalisationProvider, Logger);
            await service.StartAsync(CancellationToken.None);

            var response = await service.GetResponseAsync(Member2Id, GuildId, LocalisationStringKey.PING_COMMAND);
            Assert.False(string.IsNullOrWhiteSpace(response));

            using var scope = this._provider.CreateScope();
            await using var context = scope.ServiceProvider.GetRequiredService<EspeonDbContext>();

            var localisation = await context.UserLocalisations.FindAsync(GuildId.RawValue, Member2Id.RawValue);
            Assert.NotNull(localisation);
        }
        
        [Test]
        public async Task TestGetResponseTooManyArgs() {
            var service = new LocalisationService(this._provider, LocalisationProvider, Logger);
            await service.StartAsync(CancellationToken.None);

            Assert.DoesNotThrowAsync(async () => await service.GetResponseAsync(Member1Id, GuildId, LocalisationStringKey.REMINDER_CREATED, "1", "2"));
        }
        
        [Test]
        public async Task TestGetResponseTooFewArgs() {
            var service = new LocalisationService(this._provider, LocalisationProvider, Logger);
            await service.StartAsync(CancellationToken.None);

            Assert.DoesNotThrowAsync(async () => await service.GetResponseAsync(Member1Id, GuildId, LocalisationStringKey.REMINDER_CREATED));
        }
        
        [Test]
        public async Task TestGetResponseFormatsAsync() {
            const string espeon = "espeon";
            var service = new LocalisationService(this._provider, LocalisationProvider, Logger);
            await service.StartAsync(CancellationToken.None);

            var response = await service.GetResponseAsync(Member1Id, GuildId, LocalisationStringKey.REMINDER_CREATED, espeon);
            Assert.AreEqual(espeon, response);
        }
        
        [Test]
        public async Task TestGetResponseFallbacksAsync() {
            var service = new LocalisationService(this._provider, LocalisationProvider, Logger);
            await service.StartAsync(CancellationToken.None);

            var response = await service.GetResponseAsync(Member3Id, GuildId, LocalisationStringKey.PING_COMMAND);
            Assert.AreEqual("pong", response);
        }
        
        [Test]
        public void TestGetKeyThrowsOnInvalidKey() {
            var service = new LocalisationService(this._provider, null, Logger);
            Assert.Throws<ArgumentException>(() => service.GetKey("invalid"));
        }

        [Test]
        public void TestGetKeyThrows() {
            var service = new LocalisationService(this._provider, null, Logger);
            Assert.DoesNotThrow(() => service.GetKey(LocalisationStringKey.PING_COMMAND.ToString()));
        }
        
        [Test]
        public async Task TestStopAsync() {
            var service = new LocalisationService(this._provider, null, Logger);
            await service.StopAsync(CancellationToken.None);
        }
        
        private class TestLocalisationProvider : ILocalisationProvider {
            public ValueTask<IDictionary<Language, IDictionary<LocalisationStringKey, string>>> GetLocalisationsAsync() {
                return new ValueTask<IDictionary<Language, IDictionary<LocalisationStringKey, string>>>(
                    new Dictionary<Language, IDictionary<LocalisationStringKey, string>> {
                        [Language.Default] = new Dictionary<LocalisationStringKey, string> {
                            [LocalisationStringKey.PING_COMMAND] ="pong",
                            [LocalisationStringKey.REMINDER_CREATED] = "{0}"
                        }
                    });
            }
        }
    }
}