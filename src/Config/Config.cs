using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Espeon {
    //TODO don't use the third world serialiser that requires mutable models
    public class Config {
        public DiscordConfig Discord { get; set; }
        public PostgresConfig Postgres { get; set; }

        private Config() { }

        public static async Task<Config> FromJsonFileAsync(string fileDir) {
            await using var json = File.OpenRead(fileDir);
            return await JsonSerializer.DeserializeAsync<Config>(json);
        }
        
        public class DiscordConfig {
            public string Token { get; set; }
            
            private DiscordConfig() {}
        }
        
        public class PostgresConfig {
            public string ConnectionString { get; set; }
            
            private PostgresConfig() {}
        }
    }
}