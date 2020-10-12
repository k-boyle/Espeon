using Disqord;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Espeon.Test {
    public class LocalisationServiceTests {
        private static readonly Snowflake Member1Id = 0L;
        private static readonly Snowflake Member2Id = 1L;
        private static readonly Snowflake Member3Id = 2L;
        private static readonly Snowflake GuildId = 0L;
        private static readonly ILocalisationProvider LocalisationProvider = new TestLocalisationProvider();
        
        private ILogger _logger;
        private IServiceProvider _provider;
        
        [SetUp]
        public async Task BeforeEachAsync() {
            this._logger = TestLoggerFactory.Create();
            this._provider = new ServiceCollection()
                .AddSingleton(this._logger)
                .AddDbContext<EspeonDbContext>(builder => builder.UseInMemoryDatabase("espeon"))
                .BuildServiceProvider();
            
            using var scope = this._provider.CreateScope();
            await using var context = scope.ServiceProvider.GetService<EspeonDbContext>();

            await context.UserLocalisations.AddAsync(new UserLocalisation(Member1Id, GuildId));
            await context.UserLocalisations.AddAsync(new UserLocalisation(Member3Id, GuildId) {
                Value = Localisation.Owo
            });
            await context.SaveChangesAsync();
        }
        
        [TearDown]
        public async Task TearDownAsync() {
            using var scope = this._provider.CreateScope();
            await using var context = scope.ServiceProvider.GetService<EspeonDbContext>();
            await context.Database.EnsureDeletedAsync();
        }
        
        [Test]
        public async Task TestGetResponseForLocalisationInDbAsync() {
            var service = new LocalisationService(this._provider, LocalisationProvider, this._logger);
            await service.InitialiseAsync();

            var response = await service.GetResponseAsync(Member1Id, GuildId, LocalisationStringKey.PING_COMMAND);
            Assert.False(string.IsNullOrWhiteSpace(response));
        }
        
        [Test]
        public async Task TestGetResponseForLocalisationNotInDbAsync() {
            var service = new LocalisationService(this._provider, LocalisationProvider, this._logger);
            await service.InitialiseAsync();

            var response = await service.GetResponseAsync(Member2Id, GuildId, LocalisationStringKey.PING_COMMAND);
            Assert.False(string.IsNullOrWhiteSpace(response));

            using var scope = this._provider.CreateScope();
            await using var context = scope.ServiceProvider.GetService<EspeonDbContext>();

            var localisation = await context.UserLocalisations.FindAsync(GuildId.RawValue, Member2Id.RawValue);
            Assert.NotNull(localisation);
        }
        
        [Test]
        public async Task TestGetResponseTooManyArgs() {
            var service = new LocalisationService(this._provider, LocalisationProvider, this._logger);
            await service.InitialiseAsync();

            Assert.DoesNotThrowAsync(async () => await service.GetResponseAsync(Member1Id, GuildId, LocalisationStringKey.REMINDER_CREATED, "1", "2"));
        }
        
        [Test]
        public async Task TestGetResponseTooFewArgs() {
            var service = new LocalisationService(this._provider, LocalisationProvider, this._logger);
            await service.InitialiseAsync();

            Assert.DoesNotThrowAsync(async () => await service.GetResponseAsync(Member1Id, GuildId, LocalisationStringKey.REMINDER_CREATED));
        }
        
        [Test]
        public async Task TestGetResponseFormatsAsync() {
            const string espeon = "espeon";
            var service = new LocalisationService(this._provider, LocalisationProvider, this._logger);
            await service.InitialiseAsync();

            var response = await service.GetResponseAsync(Member1Id, GuildId, LocalisationStringKey.REMINDER_CREATED, espeon);
            Assert.AreEqual(espeon, response);
        }
        
        [Test]
        public async Task TestGetResponseFallbacksAsync() {
            var service = new LocalisationService(this._provider, LocalisationProvider, this._logger);
            await service.InitialiseAsync();

            var response = await service.GetResponseAsync(Member3Id, GuildId, LocalisationStringKey.PING_COMMAND);
            Assert.AreEqual("pong", response);
        }
        
        [Test]
        public void TestGetKeyThrowsOnInvalidKey() {
            var service = new LocalisationService(this._provider, null, this._logger);
            Assert.Throws<ArgumentException>(() => service.GetKey("invalid"));
        }

        [Test]
        public void TestGetKeyThrows() {
            var service = new LocalisationService(this._provider, null, this._logger);
            Assert.DoesNotThrow(() => service.GetKey(LocalisationStringKey.PING_COMMAND.ToString()));
        }
        
        private class TestLocalisationProvider : ILocalisationProvider {
            public ValueTask<IDictionary<Localisation, IDictionary<LocalisationStringKey, string>>> GetLocalisationsAsync() {
                return new ValueTask<IDictionary<Localisation, IDictionary<LocalisationStringKey, string>>>(
                    new Dictionary<Localisation, IDictionary<LocalisationStringKey, string>> {
                        [Localisation.Default] = new Dictionary<LocalisationStringKey, string> {
                            [LocalisationStringKey.PING_COMMAND] ="pong",
                            [LocalisationStringKey.REMINDER_CREATED] = "{0}"
                        }
                    });
            }
        }
    }
}