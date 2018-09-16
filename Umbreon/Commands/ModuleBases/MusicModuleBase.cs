using Discord.Commands;
using System.Threading.Tasks;
using Umbreon.Core.Entities.Guild;
using Umbreon.Services;

namespace Umbreon.Commands.ModuleBases
{
    public abstract class MusicModuleBase<T> : UmbreonBase<T> where T : class, ICommandContext
    {
        public MusicService Music { get; set; }
        public DatabaseService Database { get; set; }

        public Task<GuildObject> CurrentGuild => Database.GetObjectAsync<GuildObject>("guilds", Context.Guild.Id);
    }
}
