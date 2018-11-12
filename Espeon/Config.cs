using Newtonsoft.Json;
using System.IO;

namespace Espeon
{
    public class Config
    {
        public string DiscordToken { get; set; }
        public string PushbulletToken { get; set; }

        private Config()
        {
        }

        public static Config Create(string dir)
        {
            var config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(dir));
            return config;
        }
    }
}
