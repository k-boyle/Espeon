using System.Collections.Generic;
using Espeon.Core.Entities;
using LiteDB;

namespace Espeon.Entities
{
    public class Guild : DatabaseEntity
    {
        public Guild() { }

        [BsonId(false)]
        public override ulong Id { get; set; }
        
        public override long WhenToRemove { get; set; }

        public Configuration Config { get; set; } = new Configuration();
        public ElavatedUsers SpecialUsers { get; set; } = new ElavatedUsers();
        public Data Data { get; set; } = new Data();
        public Starboard Starboard { get; set; } = new Starboard();
    }

    public class Configuration
    {
        public ulong WelcomeChannelId { get; set; }
        public ulong DefaultRoleId { get; set; }

        public IList<string> Prefixes { get; set; } = new List<string>
        {
            "es/"
        };
        public IList<ulong> RestrictedChannels { get; set; } = new List<ulong>();
        public IList<ulong> RestrictedUsers { get; set; } = new List<ulong>();
    }

    public class ElavatedUsers
    {
        public IList<ulong> Admins { get; set; } = new List<ulong>();
        public IList<ulong> Moderators { get; set; } = new List<ulong>();
    }

    public class Data
    {
        public IList<ulong> SelfAssigningRoles { get; set; } = new List<ulong>();
        public IList<CustomCommand> Commands { get; set; } = new List<CustomCommand>();
    }

    public class CustomCommand : BaseCustomCommand
    {
        public override string Name { get; set; }
        public override string Value { get; set; }
    }

    public class Starboard
    {
        //TODO
    }
}
