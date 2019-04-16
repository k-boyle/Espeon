using Newtonsoft.Json;
using System.IO;

namespace Espeon
{
    public class Config
    {
        public string Dir { get; private set; }

        public string DiscordToken { get; set; }
        public string PushbulletToken { get; set; }
        public string GiphyAPIKey { get; set; }
        public ConnectionStrings ConnectionStrings { get; set; }

        public float RandomCandyFrequency { get; set; }
        public int RandomCandyAmount { get; set; }
        public int ClaimMin { get; set; }
        public int ClaimMax { get; set; }
        public int ClaimCooldown { get; set; }
        public int PackPrice { get; set; }

        private Config()
        {
        }

        //literally not needed I just like static creating for some reason
        public static Config Create(string dir)
        {
            var config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(dir));
            config.Dir = dir;

            return config;
        }

        public void Serialize()
        {
            File.WriteAllText(Dir, JsonConvert.SerializeObject(this, Formatting.Indented));
        }
    }

    public class ConnectionStrings
    {
        public string GuildStore { get; set; }
        public string UserStore { get; set; }
        public string CommandStore { get; set; }
    }
}
