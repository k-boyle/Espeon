using Discord.Commands;
using System.Threading.Tasks;
using Espeon.Core.Entities.Guild;
using Espeon.Services;

namespace Espeon.Commands.ModuleBases
{
    public abstract class MusicModuleBase<T> : EspeonBase<T> where T : class, ICommandContext
    {
        public MusicService Music { get; set; }
        public DatabaseService Database { get; set; }

        public Task<GuildObject> CurrentGuildAsync => Database.GetObjectAsync<GuildObject>("guilds", Context.Guild.Id);
    }
}
