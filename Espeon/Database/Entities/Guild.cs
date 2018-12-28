using System.Collections.Generic;

namespace Espeon.Database.Entities
{
    public class Guild : DatabaseEntity
    {
        public override ulong Id { get; set; }
        
        public override long WhenToRemove { get; set; }

        public Configuration Config { get; set; } = new Configuration();
        public ElevatedUsers SpecialUsers { get; set; } = new ElevatedUsers();
        public GuildData Data { get; set; } = new GuildData();
        public Starboard Starboard { get; set; } = new Starboard();
    }

    public class Configuration
    {
        public Guild Guild { get; set; }
        public ulong GuildId { get; set; }

        public ulong WelcomeChannelId { get; set; }
        public ulong DefaultRoleId { get; set; }

        public List<string> Prefixes { get; set; } = new List<string>
        {
            "es/"
        };
        public ICollection<ulong> RestrictedChannels { get; set; }
        public ICollection<ulong> RestrictedUsers { get; set; }
    }

    public class ElevatedUsers
    {
        public Guild Guild { get; set; }
        public ulong GuildId { get; set; }

        public ICollection<ulong> Admins { get; set; }
        public ICollection<ulong> Moderators { get; set; }
    }

    public class GuildData
    {
        public Guild Guild { get; set; }
        public ulong GuildId { get; set; }

        public ICollection<ulong> SelfAssigningRoles { get; set; }
        public List<CustomCommand> Commands { get; set; } = new List<CustomCommand>();
    }

    public class CustomCommand
    {
        public string Id { get; set; }

        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class Starboard
    {
        public Guild Guild { get; set; }
        public ulong GuildId { get; set; }

        //TODO
    }
}
