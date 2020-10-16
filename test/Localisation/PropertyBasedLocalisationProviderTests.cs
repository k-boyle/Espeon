using Microsoft.Extensions.Options;
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
            var nullLocalisationPathConfig = Options.Create(new Localisation {
                Path = null
            });
            var provider = new PropertyBasedLocalisationProvider(nullLocalisationPathConfig, this._logger);
            Assert.ThrowsAsync<InvalidOperationException>(async () => await provider.GetLocalisationsAsync());
        }
        
        [Test]
        public void TestInitialiseThrowsOnEmptyLocalisationPath() {
            var emptyLocalisationPathConfig = Options.Create(new Localisation {
               Path = string.Empty
            });
            var provider = new PropertyBasedLocalisationProvider(emptyLocalisationPathConfig, this._logger);
            Assert.ThrowsAsync<InvalidOperationException>(async () => await provider.GetLocalisationsAsync());
        }
        
        [Test]
        public void TestInvalidLocalisationFileNameThrows() {
            var invalidLocalisationPathConfig = Options.Create(new Localisation {
                Path = "./LocalisationInvalidFileName"
            });
            var provider = new PropertyBasedLocalisationProvider(invalidLocalisationPathConfig, this._logger);
            Assert.ThrowsAsync<InvalidOperationException>(async () => await provider.GetLocalisationsAsync());
        }
        
        [Test]
        public void TestExecludeLocalisationFile() {
            var localisationExlusionPathConfig = Options.Create(new Localisation {
                Path = "./LocalisationInvalidFileName",
                ExcludedFiles = new HashSet<string> {
                    "invalid"
                }
            });
            var provider = new PropertyBasedLocalisationProvider(localisationExlusionPathConfig, this._logger);
            Assert.DoesNotThrowAsync(async () => await provider.GetLocalisationsAsync());
        }
        
        [Test]
        public void TestExecludeLocalisationRegex() {
            var localisationExlusionPathConfig = Options.Create(new Localisation {
                Path = "./LocalisationInvalidFileName",
                ExclusionRegex = "invalid"
            });
            var provider = new PropertyBasedLocalisationProvider(localisationExlusionPathConfig, this._logger);
            Assert.DoesNotThrowAsync(async () => await provider.GetLocalisationsAsync());
        }
        
        [Test]
        public void TestInvalidLocalisationString() {
            var localisationExlusionPathConfig = Options.Create(new Localisation {
                Path = "./LocalisationInvalidLocalisationString"
            });
            var provider = new PropertyBasedLocalisationProvider(localisationExlusionPathConfig, this._logger);
            Assert.ThrowsAsync<InvalidOperationException>(async () => await provider.GetLocalisationsAsync());
        }
        
        [Test]
        public void TestInvalidLocalisationStringKey() {
            var localisationExlusionPathConfig = Options.Create(new Localisation {
                    Path = "./LocalisationInvalidLocalisationStringKey"
            });
            var provider = new PropertyBasedLocalisationProvider(localisationExlusionPathConfig, this._logger);
            Assert.ThrowsAsync<InvalidOperationException>(async () => await provider.GetLocalisationsAsync());
        }
        
        [Test]
        public async Task TestGetLocalisationReturnsCorrectValuesAsync() {
            var validLocalisationConfig = Options.Create(new Localisation {
                    Path = "./LocalisationValid"
            });
            var provider = new PropertyBasedLocalisationProvider(validLocalisationConfig, this._logger);
            var result = await provider.GetLocalisationsAsync();
            var expected = new Dictionary<Language, Dictionary<LocalisationStringKey, string>> {
                [Language.Default] = new Dictionary<LocalisationStringKey, string> {
                    [LocalisationStringKey.PING_COMMAND] ="pong",
                    [LocalisationStringKey.REMINDER_CREATED] = "{0}"
                },
                [Language.Owo] = new Dictionary<LocalisationStringKey, string>()
            };
            
            CollectionAssert.AreEquivalent(expected, result);
        }
    }
}