using Espeon.Core.Commands.Bases;
using Espeon.Core.Services;
using Qmmands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Core.Commands.Modules
{
    [Name("Reminder")]
    [Group("Reminder")]
    public class Reminder : EspeonBase
    {
        public IReminderService ReminderService { get; set; }

        [Command]
        public async Task<EspeonResult> CreateReminderAsync(TimeSpan when, [Remainder] string reminder)
        {
            await ReminderService.CreateReminderAsync(Context, reminder, when);

            return new EspeonResult(true, "Reminder has been created!");
        }

        //TODO Make better
        [Command("List")]
        public async Task<EspeonResult> ListRemindersAsync()
        {
            var reminders = await ReminderService.GetRemindersAsync(Context);
            var ordered = reminders.OrderBy(x => x.WhenToRemove);
            
            return new EspeonResult(true, string.Join('\n', ordered.Select(x => $"{x.Id}: {x.TheReminder}")));
        }
    }
}
