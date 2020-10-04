using Serilog;
using Serilog.Events;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Espeon {
    //TODO don't use the third world serialiser that requires mutable models
    public class Config {
        public DiscordConfig Discord { get; set; }
        public PostgresConfig Postgres { get; set; }
        public LoggingConfig Logging { get; set; }
        public LocalisationConfig Localisation { get; set; }

        public static async Task<Config> FromJsonFileAsync(string fileDir) {
            await using var json = File.OpenRead(fileDir);
            var options = new JsonSerializerOptions();
            options.Converters.Add(new JsonStringEnumConverter());
            return await JsonSerializer.DeserializeAsync<Config>(json, options);
        }

        public class DiscordConfig {
            public string Token { get; set; }
        }

        public class PostgresConfig {
            public string ConnectionString { get; set; }
        }

        public class LoggingConfig {
            public bool WriteToFile { get; set; }
            public bool WriteToConsole { get; set; }
            public string Path { get; set; }
            public LogEventLevel Level { get; set; }
            public RollingInterval RollingInterval { get; set; }
        }

        public class LocalisationConfig {
            public string Path { get; set; }
            public HashSet<string> ExcludedFiles { get; set; }
            public string ExclusionRegex { get; set; }
        }
    }
}