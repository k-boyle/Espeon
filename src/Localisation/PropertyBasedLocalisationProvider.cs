using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Espeon {
    public class PropertyBasedLocalisationProvider : ILocalisationProvider {
        private readonly HashSet<string> _excludedFiles;
        private readonly Regex _exclusionRegex;
        private readonly string _localisationPath;
        private readonly ILogger _logger;

        public PropertyBasedLocalisationProvider(IOptions<Localisation> localisationOptions, ILogger logger) {
            var config = localisationOptions.Value;
            this._excludedFiles = config.ExcludedFiles;
            this._exclusionRegex = GetExclusionRegex(config);
            this._localisationPath = config.Path;
            this._logger = logger.ForContext("SourceContext", nameof(PropertyBasedLocalisationProvider));
        }

        private static Regex GetExclusionRegex(Localisation config) {
            return config.ExclusionRegex != null
                ? new Regex(config.ExclusionRegex, RegexOptions.Compiled)
                : null;
        }

        public async ValueTask<IDictionary<Language, IDictionary<LocalisationStringKey, string>>> GetLocalisationsAsync() {
            this._logger.Debug("Reading localisation strings from property files");
            var responses = new Dictionary<Language, IDictionary<LocalisationStringKey, string>>();
            
            if (string.IsNullOrWhiteSpace(this._localisationPath)) {
                throw new InvalidOperationException("Language config must be defined");
            }
            
            var fullPathFiles = Directory.GetFiles(this._localisationPath);
            foreach (var fullPath in fullPathFiles) {
                var fileName = Path.GetFileName(fullPath);
                this._logger.Debug("Reading property file {fileName}", fileName);
                
                if (!IsLanguageFile(fileName, out var language)) {
                    continue;
                }

                var responsesForFile = await ReadFileAsync(fullPath);
                responses[language] = responsesForFile;
            }

            return responses;
        }

        private bool IsLanguageFile(string fileName, out Language language) {
            language = Language.Default;
            if (this._excludedFiles?.Contains(fileName) == true || this._exclusionRegex?.IsMatch(fileName) == true) {
                return false;
            }

            if (!Enum.TryParse(fileName, true, out language)) {
                throw new InvalidOperationException($"{fileName} was not recognised as a valid localisation");
            }

            return true;
        }

        private async Task<ConcurrentDictionary<LocalisationStringKey, string>> ReadFileAsync(string fullPath) {
            var responsesForFile = new ConcurrentDictionary<LocalisationStringKey, string>();
            var lines = await File.ReadAllLinesAsync(fullPath);
            foreach (var line in lines) {
                var response = ParseLine(line, out var key);
                responsesForFile[key] = response[1];
            }

            return responsesForFile;
        }

        private string[] ParseLine(string line, out LocalisationStringKey key) {
            this._logger.Debug("Parsing line {line}", line);

            var split = line.Split('=', StringSplitOptions.RemoveEmptyEntries);
            if (split.Length != 2) {
                throw new InvalidOperationException("Language string must have format \"key=value\"");
            }

            if (!Enum.TryParse(split[0], out key)) {
                throw new InvalidOperationException($"{split[0]} was not recognised as a valid localisation key");
            }

            return split;
        }
    }
}