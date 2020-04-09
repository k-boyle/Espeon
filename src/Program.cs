using System.Threading.Tasks;

namespace Espeon
{
    class Program
    {
        //TODO config from args
        static async Task Main(string[] args) {
            var config = await Config.FromJsonFileAsync("./config.json");
        }
    }
}
