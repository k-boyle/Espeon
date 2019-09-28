using Casino.DependencyInjection;
using Discord;
using Discord.WebSocket;
using Espeon.Services;
using Qmmands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Bot.Services
{
    public class StatusService : BaseService<InitialiseArgs>, IStatusService
    {
        [Inject] private readonly CommandService _commands;
        [Inject] private readonly DiscordSocketClient _client;
        [Inject] private readonly Random _random;

        private static readonly TimeSpan Delay = TimeSpan.FromMinutes(1);

        public StatusService(IServiceProvider services) : base(services)
        {
        }

        async Task IStatusService.RunStatusesAsync()
        {
            var last = -1;

            while (true)
            {
                var statuses = new[]
                {
                    (ActivityType.Watching, "you"),
                    (ActivityType.Playing, $"with {_client.Guilds.Sum(x => x.MemberCount)} people"),
                    (ActivityType.Listening, "Pokemon opening theme"),
                    (ActivityType.Watching, $"over {_commands.GetAllCommands().Count} commands")
                };

                int next;

                do
                {
                    next = _random.Next(statuses.Length);
                } while (next == last);

                var (activityType, str) = statuses[next];

                await _client.SetGameAsync(str, "", activityType);

                last = next;
                await Task.Delay(Delay);
            }
        }
    }
}
