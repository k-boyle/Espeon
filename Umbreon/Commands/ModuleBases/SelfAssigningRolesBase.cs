using Discord.Commands;
using System.Collections.Generic;
using Umbreon.Services;

namespace Umbreon.Commands.ModuleBases
{
    public class SelfAssigningRolesBase<T> : UmbreonBase<T> where T : class, ICommandContext
    {
        public IEnumerable<ulong> CurrentRoles { get; private set; }
        public SelfAssigningRolesService SelfRoles { get; set; }

        protected override void BeforeExecute(CommandInfo command)
        {
            CurrentRoles = SelfRoles.GetRoles(Context);
        }
    }
}
