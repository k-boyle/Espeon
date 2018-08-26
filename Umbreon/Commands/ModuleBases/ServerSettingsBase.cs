using Discord.Commands;
using Umbreon.Core.Entities.Guild;
using Umbreon.Services;

namespace Umbreon.Commands.ModuleBases
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
