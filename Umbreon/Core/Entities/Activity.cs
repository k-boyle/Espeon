using Discord;

namespace Umbreon.Core.Entities
{
    public class Activity : IActivity
    {
        public string Name { get; }
        public ActivityType Type { get; }

        public Activity(string name, ActivityType type)
        {
            Name = name;
            Type = type;
        }
    }
}
