using Discord.Commands;
using System;
using Umbreon.Core.Entities.Guild;
using Umbreon.Services;

namespace Umbreon.Commands.ModuleBases
{
    public abstract class MusicModuleBase<T> : UmbreonBase<T> where T : class, ICommandContext
    {
        public MusicService Music { get; set; }
        public DatabaseService Database { get; set; }
        public GuildObject CurrentGuild { get; private set; }

        protected override void BeforeExecute(CommandInfo command)
        {
            CurrentGuild = Database.GetObject<GuildObject>("guilds", Context.Guild.Id);
        }

        protected override void AfterExecute(CommandInfo command)
        {
            if(command.Name.Equals("Approve User", StringComparison.CurrentCultureIgnoreCase))
                Database.UpdateObject("guilds", CurrentGuild);
        }
    }
}
