using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace Espeon.Test {
    public class PropertyBasedLocalisationProviderTests {
        private static readonly ILogger<PropertyBasedLocalisationProvider> Logger = new NullLogger<PropertyBasedLocalisationProvider>();

        [Test]
        public void TestInitialiseThrowsOnNullLocalisationPath() {
            var nullLocalisationPathConfig = Options.Create(new Localisation {
                Path = null
            });
            var provider = new PropertyBasedLocalisationProvider(nullLocalisationPathConfig, Logger);
            Assert.ThrowsAsync<InvalidOperationException>(async () => await provider.GetLocalisationsAsync());
        }
        
        [Test]
        public void TestInitialiseThrowsOnEmptyLocalisationPath() {
            var emptyLocalisationPathConfig = Options.Create(new Localisation {
               Path = string.Empty
            });
            var provider = new PropertyBasedLocalisationProvider(emptyLocalisationPathConfig, Logger);
            Assert.ThrowsAsync<InvalidOperationException>(async () => await provider.GetLocalisationsAsync());
        }
        
        [Test]
        public void TestInvalidLocalisationFileNameThrows() {
            var invalidLocalisationPathConfig = Options.Create(new Localisation {
                Path = "./LocalisationInvalidFileName"
            });
            var provider = new PropertyBasedLocalisationProvider(invalidLocalisationPathConfig, Logger);
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
            var provider = new PropertyBasedLocalisationProvider(localisationExlusionPathConfig, Logger);
            Assert.DoesNotThrowAsync(async () => await provider.GetLocalisationsAsync());
        }
        
        [Test]
        public void TestExecludeLocalisationRegex() {
            var localisationExlusionPathConfig = Options.Create(new Localisation {
                Path = "./LocalisationInvalidFileName",
                ExclusionRegex = "invalid"
            });
            var provider = new PropertyBasedLocalisationProvider(localisationExlusionPathConfig, Logger);
            Assert.DoesNotThrowAsync(async () => await provider.GetLocalisationsAsync());
        }
        
        [Test]
        public void TestInvalidLocalisationString() {
            var localisationExlusionPathConfig = Options.Create(new Localisation {
                Path = "./LocalisationInvalidLocalisationString"
            });
            var provider = new PropertyBasedLocalisationProvider(localisationExlusionPathConfig, Logger);
            Assert.ThrowsAsync<InvalidOperationException>(async () => await provider.GetLocalisationsAsync());
        }
        
        [Test]
        public void TestInvalidLocalisationStringKey() {
            var localisationExlusionPathConfig = Options.Create(new Localisation {
                    Path = "./LocalisationInvalidLocalisationStringKey"
            });
            var provider = new PropertyBasedLocalisationProvider(localisationExlusionPathConfig, Logger);
            Assert.ThrowsAsync<InvalidOperationException>(async () => await provider.GetLocalisationsAsync());
        }
        
        [Test]
        public async Task TestGetLocalisationReturnsCorrectValuesAsync() {
            var validLocalisationConfig = Options.Create(new Localisation {
                    Path = "./LocalisationValid"
            });
            var provider = new PropertyBasedLocalisationProvider(validLocalisationConfig, Logger);
            var result = await provider.GetLocalisationsAsync();
            var expected = new Dictionary<Language, Dictionary<LocalisationStringKey, string>> {
                [Language.Default] = new() {
                    [LocalisationStringKey.PING_COMMAND] ="pong",
                    [LocalisationStringKey.REMINDER_CREATED] = "{0}"
                },
                [Language.Owo] = new()
            };
            
            CollectionAssert.AreEquivalent(expected, result);
        }
    }
}