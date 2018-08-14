using Discord.Commands;
using Umbreon.Core.Models.Database.Guilds;
using Umbreon.Services;

namespace Umbreon.Modules.ModuleBases
{
    public class ServerSettingsBase<T> : UmbreonBase<T> where T : class, ICommandContext
    {
        public DatabaseService Database { get; set; }
        [DontInject]
        public GuildObject CurrentGuild { get; private set; }

        protected override void BeforeExecute(CommandInfo command)
        {
            CurrentGuild = Database.GetGuild(Context);
        }

        protected override void AfterExecute(CommandInfo command)
        {
            Database.UpdateGuild(CurrentGuild);
        }
    }
}
