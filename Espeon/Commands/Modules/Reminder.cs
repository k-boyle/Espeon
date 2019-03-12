using Espeon.Services;
using Qmmands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Commands
{
    [Name("Reminders")]
    [Group("Reminder")]
    public class Reminder : EspeonBase
    {
        public ReminderService ReminderService { get; set; }

        [Command]
        [Name("Reminder")]
        public Task CreateReminderAsync(TimeSpan when, [Remainder] string reminder)
        {
            return Task.WhenAll(ReminderService.CreateReminderAsync(Context, reminder, when), SendOkAsync(0));
        }

        //TODO Make better
        [Command("List")]
        [Name("List Reminders")]
        public async Task ListRemindersAsync()
        {
            var reminders = await ReminderService.GetRemindersAsync(Context);
            var ordered = reminders.OrderBy(x => x.WhenToRemove);

            var message = string.Join('\n', ordered.Select(x => $"{x.ReminderId}: {x.TheReminder}"));
            var response = ResponseBuilder.Message(Context, message);
            
            await SendMessageAsync(response);
        }
    }
}
