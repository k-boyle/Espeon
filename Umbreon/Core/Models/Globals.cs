using System;
using Umbreon.Modules.Contexts;
using Umbreon.Services;

namespace Umbreon.Core.Models
{
    public class Globals
    {
        public GuildCommandContext Context { get; set; }
        public IServiceProvider Services { get; set; }
        public MessageService Message { get; set; }
    }
}
