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
        private static readonly Snowflake GuildId = 0L;
        
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
            await context.SaveChangesAsync();
        }
        
        [TearDown]
        public async Task TearDownAsync() {
            using var scope = this._provider.CreateScope();
            await using var context = scope.ServiceProvider.GetService<EspeonDbContext>();
            await context.Database.EnsureDeletedAsync();
        }

        [Test]
        public void TestInitialiseThrowsOnNullLocalisation() {
            var nullLocalisationConfig = new Config { Localisation = null };
            var service = new LocalisationService(this._provider, nullLocalisationConfig, this._logger);
            Assert.ThrowsAsync<InvalidOperationException>(service.InitialiseAsync);
        }

        [Test]
        public void TestInitialiseThrowsOnNullLocalisationPath() {
            var nullLocalisationPathConfig = new Config {
                Localisation = new Config.LocalisationConfig {
                    Path = null
                }
            };
            var service = new LocalisationService(this._provider, nullLocalisationPathConfig, this._logger);
            Assert.ThrowsAsync<InvalidOperationException>(service.InitialiseAsync);
        }
        
        

        [Test]
        public void TestInitialiseThrowsOnEmptyLocalisationPath() {
            var emptyLocalisationPathConfig = new Config {
                Localisation = new Config.LocalisationConfig {
                    Path = string.Empty
                }
            };
            var service = new LocalisationService(this._provider, emptyLocalisationPathConfig, this._logger);
            Assert.ThrowsAsync<InvalidOperationException>(service.InitialiseAsync);
        }
        
        [Test]
        public void TestInvalidLocalisationFileNameThrows() {
            var invalidLocalisationPathConfig = new Config {
                Localisation = new Config.LocalisationConfig {
                    Path = "./LocalisationInvalidFileName"
                }
            };
            var service = new LocalisationService(this._provider, invalidLocalisationPathConfig, this._logger);
            Assert.ThrowsAsync<InvalidOperationException>(service.InitialiseAsync);
        }
        
        [Test]
        public void TestExecludeLocalisationFile() {
            var localisationExlusionPathConfig = new Config {
                Localisation = new Config.LocalisationConfig {
                    Path = "./LocalisationInvalidFileName",
                    ExcludedFiles = new HashSet<string> {
                        "invalid"
                    }
                }
            };
            var service = new LocalisationService(this._provider, localisationExlusionPathConfig, this._logger);
            Assert.DoesNotThrowAsync(service.InitialiseAsync);
        }
        
        [Test]
        public void TestExecludeLocalisationRegex() {
            var localisationExlusionPathConfig = new Config {
                Localisation = new Config.LocalisationConfig {
                    Path = "./LocalisationInvalidFileName",
                    ExclusionRegex = "invalid"
                }
            };
            var service = new LocalisationService(this._provider, localisationExlusionPathConfig, this._logger);
            Assert.DoesNotThrowAsync(service.InitialiseAsync);
        }
        
        [Test]
        public void TestInvalidLocalisationString() {
            var localisationExlusionPathConfig = new Config {
                Localisation = new Config.LocalisationConfig {
                    Path = "./LocalisationInvalidLocalisationString"
                }
            };
            var service = new LocalisationService(this._provider, localisationExlusionPathConfig, this._logger);
            Assert.ThrowsAsync<InvalidOperationException>(service.InitialiseAsync);
        }
        
        
        
        [Test]
        public void TestInvalidLocalisationStringKey() {
            var localisationExlusionPathConfig = new Config {
                Localisation = new Config.LocalisationConfig {
                    Path = "./LocalisationInvalidLocalisationStringKey"
                }
            };
            var service = new LocalisationService(this._provider, localisationExlusionPathConfig, this._logger);
            Assert.ThrowsAsync<InvalidOperationException>(service.InitialiseAsync);
        }
        
        [Test]
        public async Task TestGetResponseForLocalisationInDbAsync() {
            var validLocalisationConfig = new Config {
                Localisation = new Config.LocalisationConfig {
                    Path = "./LocalisationValid"
                }
            };
            var service = new LocalisationService(this._provider, validLocalisationConfig, this._logger);
            await service.InitialiseAsync();

            var response = await service.GetResponseAsync(Member1Id, GuildId, LocalisationStringKey.PING_COMMAND);
            Assert.False(string.IsNullOrWhiteSpace(response));
        }
        
        [Test]
        public async Task TestGetResponseForLocalisationNotInDbAsync() {
            var validLocalisationConfig = new Config {
                Localisation = new Config.LocalisationConfig {
                    Path = "./LocalisationValid"
                }
            };
            var service = new LocalisationService(this._provider, validLocalisationConfig, this._logger);
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
            var validLocalisationConfig = new Config {
                Localisation = new Config.LocalisationConfig {
                    Path = "./LocalisationValid"
                }
            };
            var service = new LocalisationService(this._provider, validLocalisationConfig, this._logger);
            await service.InitialiseAsync();

            Assert.DoesNotThrowAsync(async () => await service.GetResponseAsync(Member1Id, GuildId, LocalisationStringKey.REMINDER_CREATED, "1", "2"));
        }
        
        [Test]
        public async Task TestGetResponseTooFewArgs() {
            var validLocalisationConfig = new Config {
                Localisation = new Config.LocalisationConfig {
                    Path = "./LocalisationValid"
                }
            };
            var service = new LocalisationService(this._provider, validLocalisationConfig, this._logger);
            await service.InitialiseAsync();

            Assert.DoesNotThrowAsync(async () => await service.GetResponseAsync(Member1Id, GuildId, LocalisationStringKey.REMINDER_CREATED));
        }
        
        [Test]
        public async Task TestGetResponseFormatsAsync() {
            const string espeon = "espeon";
            
            var validLocalisationConfig = new Config {
                Localisation = new Config.LocalisationConfig {
                    Path = "./LocalisationValid"
                }
            };
            var service = new LocalisationService(this._provider, validLocalisationConfig, this._logger);
            await service.InitialiseAsync();

            var response = await service.GetResponseAsync(Member1Id, GuildId, LocalisationStringKey.REMINDER_CREATED, espeon);
            Assert.AreEqual(espeon, response);
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
    }
}