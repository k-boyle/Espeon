using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Espeon {
    public class PropertyBasedLocalisationProvider : ILocalisationProvider {
        private readonly Config _config;
        private readonly ILogger _logger;

        public PropertyBasedLocalisationProvider(Config config, ILogger logger) {
            this._config = config;
            this._logger = logger.ForContext("SourceContext", nameof(PropertyBasedLocalisationProvider));
        }

        public async ValueTask<IDictionary<Localisation, IDictionary<LocalisationStringKey, string>>> GetLocalisationsAsync() {
            this._logger.Debug("Reading localisation strings from property files");
            
            var responses = new Dictionary<Localisation, IDictionary<LocalisationStringKey, string>>();
            
            if (string.IsNullOrWhiteSpace(this._config.Localisation?.Path)) {
                throw new InvalidOperationException("Localisation config must be defined");
            }
            
            var exclusionRegex = this._config.Localisation.ExclusionRegex != null
                ? new Regex(this._config.Localisation.ExclusionRegex, RegexOptions.Compiled)
                : null;
            var fullPathFiles = Directory.GetFiles(this._config.Localisation.Path);
            foreach (var fullPath in fullPathFiles) {
                var fileName = Path.GetFileName(fullPath);
                this._logger.Debug("Reading property file {fileName}", fileName);
                
                if (this._config.Localisation.ExcludedFiles?.Contains(fileName) == true
                 || exclusionRegex?.IsMatch(fileName) == true) {
                    continue;
                }

                if (!Enum.TryParse<Localisation>(fileName, true, out var localisation)) {
                    throw new InvalidOperationException($"{fileName} was not recognised as a valid localisation");
                }

                var responsesForFile = new ConcurrentDictionary<LocalisationStringKey, string>();
                var lines = await File.ReadAllLinesAsync(fullPath);
                foreach (var line in lines) {
                    this._logger.Debug("Parsing line {line}", line);
                    
                    var split = line.Split('=', StringSplitOptions.RemoveEmptyEntries);
                    if (split.Length != 2) {
                        throw new InvalidOperationException("Localisation string must have format \"key=value\"");
                    }

                    if (!Enum.TryParse<LocalisationStringKey>(split[0], out var key)) {
                        throw new InvalidOperationException($"{split[0]} was not recognised as a valid localisation key");
                    }
                    responsesForFile[key] = split[1];
                }

                responses[localisation] = responsesForFile;
            }

            return responses;
        }
    }
}