using System.Threading.Tasks;

namespace Espeon
{
    class Program
    {
        private static async Task Main()
        {
            var espeon = new EspeonStartup();
            await espeon.StartBotAsync();

            await Task.Delay(-1);
        }
    }
}
