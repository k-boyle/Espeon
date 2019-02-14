using Newtonsoft.Json;
using System.IO;

namespace Espeon
{
    public class Config
    {
        public string DiscordToken { get; set; }
        public string PushbulletToken { get; set; }
        public string GiphyAPIKey { get; set; }
        public ConnectionStrings ConnectionStrings { get; set; }

        private Config()
        {
        }

        //literally not needed I just like static creating for some reason
        public static Config Create(string dir)
        {
            var config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(dir));
            return config;
        }        
    }

    public class ConnectionStrings
    {
        public string GuildStore { get; set; }
        public string UserStore { get; set; }
        public string CommandStore { get; set; }
    }
}
