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
        private readonly IOptions<Localisation> _localisationOptions;
        private readonly ILogger _logger;

        public PropertyBasedLocalisationProvider(IOptions<Localisation> localisationOptions, ILogger logger) {
            this._localisationOptions = localisationOptions;
            this._logger = logger.ForContext("SourceContext", nameof(PropertyBasedLocalisationProvider));
        }

        public async ValueTask<IDictionary<Language, IDictionary<LocalisationStringKey, string>>> GetLocalisationsAsync() {
            this._logger.Debug("Reading localisation strings from property files");
            var config = this._localisationOptions.Value;
            
            var responses = new Dictionary<Language, IDictionary<LocalisationStringKey, string>>();
            
            if (string.IsNullOrWhiteSpace(config.Path)) {
                throw new InvalidOperationException("Language config must be defined");
            }
            
            var exclusionRegex = config.ExclusionRegex != null
                ? new Regex(config.ExclusionRegex, RegexOptions.Compiled)
                : null;
            var fullPathFiles = Directory.GetFiles(config.Path);
            foreach (var fullPath in fullPathFiles) {
                var fileName = Path.GetFileName(fullPath);
                this._logger.Debug("Reading property file {fileName}", fileName);
                
                if (config.ExcludedFiles?.Contains(fileName) == true
                 || exclusionRegex?.IsMatch(fileName) == true) {
                    continue;
                }

                if (!Enum.TryParse<Language>(fileName, true, out var language)) {
                    throw new InvalidOperationException($"{fileName} was not recognised as a valid localisation");
                }

                var responsesForFile = new ConcurrentDictionary<LocalisationStringKey, string>();
                var lines = await File.ReadAllLinesAsync(fullPath);
                foreach (var line in lines) {
                    this._logger.Debug("Parsing line {line}", line);
                    
                    var split = line.Split('=', StringSplitOptions.RemoveEmptyEntries);
                    if (split.Length != 2) {
                        throw new InvalidOperationException("Language string must have format \"key=value\"");
                    }

                    if (!Enum.TryParse<LocalisationStringKey>(split[0], out var key)) {
                        throw new InvalidOperationException($"{split[0]} was not recognised as a valid localisation key");
                    }
                    responsesForFile[key] = split[1];
                }

                responses[language] = responsesForFile;
            }

            return responses;
        }
    }
}