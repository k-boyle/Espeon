using Espeon.Core.Commands.Bases;
using Qmmands;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Espeon.Core.Commands.Modules
{
    [Name("Misc")]
    public class Misc : EspeonBase
    {
        [Command("Ping")]
        public async Task PingAsync()
        {
            var latency = Context.Client.Latency;
            var str = $"Latency: {latency}ms\nPing: ";
            var response = ResponseBuilder.Message(Context, str);

            var sw = new Stopwatch();
            sw.Start();

            var message = await SendMessageAsync(string.Empty, response);

            sw.Stop();

            response = ResponseBuilder.Message(Context, $"{str}{sw.ElapsedMilliseconds}ms");

            await message.ModifyAsync(x => x.Embed = response);
        }
    }
}
