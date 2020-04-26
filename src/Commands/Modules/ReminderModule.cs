using Qmmands;
using System.Threading.Tasks;
using static Espeon.LocalisationStringKey;

namespace Espeon {
    [Name("Reminders")]
    [Group("reminder", "reminders", "remindme", "rm", "r")]
    public class ReminderModule : EspeonCommandModule {
        public ReminderService ReminderService { get; set; }
        
        [Name("Create Reminder")]
        [Command("", "create")]
        public async Task CreateReminderAsync([Remainder] UserReminder reminder) {
            await ReminderService.CreateReminderAsync(reminder);
            await ReplyAsync(REMINDER_CREATED);
        }
    }
}