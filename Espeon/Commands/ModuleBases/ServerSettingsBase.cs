using System.Threading.Tasks;
using Discord.Commands;
using Espeon.Core.Entities.Guild;
using Espeon.Services;

namespace Espeon.Commands.ModuleBases
{
    public class ServerSettingsBase<T> : EspeonBase<T> where T : class, ICommandContext
    {
        public DatabaseService Database { get; set; }

        public Task<GuildObject> CurrentGuild => Database.GetObjectAsync<GuildObject>("guilds", Context.Guild.Id);
    }
}
