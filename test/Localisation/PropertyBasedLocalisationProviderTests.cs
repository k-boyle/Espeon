using NUnit.Framework;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Espeon.Test {
    public class PropertyBasedLocalisationProviderTests {
        private readonly ILogger _logger = TestLoggerFactory.Create();

        [Test]
        public void TestGetLocalisationThrowsOnNullConfig() {
            var provider = new PropertyBasedLocalisationProvider(null, this._logger);
            Assert.ThrowsAsync<NullReferenceException>(async () => await provider.GetLocalisationsAsync());
        }
        
        [Test]
        public void TestInitialiseThrowsOnNullLocalisationPath() {
            var nullLocalisationPathConfig = new Config {
                Localisation = new Config.LocalisationConfig {
                    Path = null
                }
            };
            var provider = new PropertyBasedLocalisationProvider(nullLocalisationPathConfig, this._logger);
            Assert.ThrowsAsync<InvalidOperationException>(async () => await provider.GetLocalisationsAsync());
        }
        
        [Test]
        public void TestInitialiseThrowsOnEmptyLocalisationPath() {
            var emptyLocalisationPathConfig = new Config {
                Localisation = new Config.LocalisationConfig {
                    Path = string.Empty
                }
            };
            var provider = new PropertyBasedLocalisationProvider(emptyLocalisationPathConfig, this._logger);
            Assert.ThrowsAsync<InvalidOperationException>(async () => await provider.GetLocalisationsAsync());
        }
        
        [Test]
        public void TestInvalidLocalisationFileNameThrows() {
            var invalidLocalisationPathConfig = new Config {
                Localisation = new Config.LocalisationConfig {
                    Path = "./LocalisationInvalidFileName"
                }
            };
            var provider = new PropertyBasedLocalisationProvider(invalidLocalisationPathConfig, this._logger);
            Assert.ThrowsAsync<InvalidOperationException>(async () => await provider.GetLocalisationsAsync());
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
            var provider = new PropertyBasedLocalisationProvider(localisationExlusionPathConfig, this._logger);
            Assert.DoesNotThrowAsync(async () => await provider.GetLocalisationsAsync());
        }
        
        [Test]
        public void TestExecludeLocalisationRegex() {
            var localisationExlusionPathConfig = new Config {
                Localisation = new Config.LocalisationConfig {
                    Path = "./LocalisationInvalidFileName",
                    ExclusionRegex = "invalid"
                }
            };
            var provider = new PropertyBasedLocalisationProvider(localisationExlusionPathConfig, this._logger);
            Assert.DoesNotThrowAsync(async () => await provider.GetLocalisationsAsync());
        }
        
        [Test]
        public void TestInvalidLocalisationString() {
            var localisationExlusionPathConfig = new Config {
                Localisation = new Config.LocalisationConfig {
                    Path = "./LocalisationInvalidLocalisationString"
                }
            };
            var provider = new PropertyBasedLocalisationProvider(localisationExlusionPathConfig, this._logger);
            Assert.ThrowsAsync<InvalidOperationException>(async () => await provider.GetLocalisationsAsync());
        }
        
        [Test]
        public void TestInvalidLocalisationStringKey() {
            var localisationExlusionPathConfig = new Config {
                Localisation = new Config.LocalisationConfig {
                    Path = "./LocalisationInvalidLocalisationStringKey"
                }
            };
            var provider = new PropertyBasedLocalisationProvider(localisationExlusionPathConfig, this._logger);
            Assert.ThrowsAsync<InvalidOperationException>(async () => await provider.GetLocalisationsAsync());
        }
        
        [Test]
        public async Task TestGetLocalisationReturnsCorrectValuesAsync() {
            var validLocalisationConfig = new Config {
                Localisation = new Config.LocalisationConfig {
                    Path = "./LocalisationValid"
                }
            };
            
            var provider = new PropertyBasedLocalisationProvider(validLocalisationConfig, this._logger);
            var result = await provider.GetLocalisationsAsync();
            var expected = new Dictionary<Localisation, Dictionary<LocalisationStringKey, string>> {
                [Localisation.Default] = new Dictionary<LocalisationStringKey, string> {
                    [LocalisationStringKey.PING_COMMAND] ="pong",
                    [LocalisationStringKey.REMINDER_CREATED] = "{0}"
                },
                [Localisation.Owo] = new Dictionary<LocalisationStringKey, string>()
            };
            
            CollectionAssert.AreEquivalent(expected, result);
        }
    }
}