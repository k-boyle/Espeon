using System.Threading.Tasks;
using Discord.Commands;
using Umbreon.Core.Entities.Guild;
using Umbreon.Services;

namespace Umbreon.Commands.ModuleBases
{
    public class ServerSettingsBase<T> : UmbreonBase<T> where T : class, ICommandContext
    {
        public DatabaseService Database { get; set; }

        public Task<GuildObject> CurrentGuild => Database.GetObjectAsync<GuildObject>("guilds", Context.Guild.Id);
    }
}
