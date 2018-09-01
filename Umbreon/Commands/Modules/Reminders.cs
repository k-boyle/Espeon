using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Umbreon.Attributes;
using Umbreon.Commands.ModuleBases;
using Umbreon.Services;

namespace Umbreon.Commands.Modules
{
    [Name("Reminders")]
    [Summary("Need to be reminded? These are your commands")]
    public class Reminders : UmbreonBase
    {
        private readonly RemindersService _reminders;
        private readonly DatabaseService _database;

        public Reminders(RemindersService reminders, DatabaseService database)
        {
            _reminders = reminders;
            _database = database;
        }

        [Command("Reminder")]
        [Name("Set Reminder")]
        [Summary("Set a reminder for yourself")]
        [Usage("reminder 30m get dinner out of the oven")]
        [Alias("remindme")]
        public async Task Reminder(
            [Name("When")]
            [Summary("The time at which you want to be reminded. #d#h#m#s")] TimeSpan when, 
            [Name("Reminder")]
            [Summary("What you want to be reminded about")]
            [Remainder] string content)
        {
            _reminders.CreateReminder($"{content}\n\n{Context.Message.GetJumpUrl()}", Context.Guild.Id, Context.Channel.Id, Context.User.Id, when);
            await SendMessageAsync("Reminder has been created");
        }

        [Command("Reminders")]
        [Name("List Reminders")]
        [Summary("List all of your reminders")]
        [Usage("reminders")]
        public async Task ListReminders()
        {
            var reminders = _database.GetGuild(Context.Guild.Id).Reminders;
            var users = reminders.Where(x => x.UserId == Context.User.Id);
            var i = 1;
            await SendMessageAsync($"Your current reminders are:\n" +
                                   $"{(users.Count() > 0 ? string.Join("\n", users.Select(x => (x.TheReminder.Length > 100 ? $"**{i++}**: {x.TheReminder.Substring(0, 100)}..." : $"**{i++}**:{x.TheReminder}"))) : "None")}");
        }
    }
}
