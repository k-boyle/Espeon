using Espeon.Services;
using Qmmands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Commands.Modules
{
    [Name("Reminder")]
    [Group("Reminder")]
    public class Reminder : EspeonBase
    {
        public ReminderService ReminderService { get; set; }

        [Command]
        [Name("Reminder")]
        public async Task CreateReminderAsync(TimeSpan when, [Remainder] string reminder)
        {
            await ReminderService.CreateReminderAsync(Context, reminder, when);

            var response = await Response.GetResponseAsync(Module, Command, ResponsePack);
            var embed = ResponseBuilder.Message(Context, response);

            await SendMessageAsync(embed);
        }

        //TODO Make better
        [Command("List")]
        [Name("List Reminders")]
        public async Task ListRemindersAsync()
        {
            var reminders = ReminderService.GetReminders(Context);
            var ordered = reminders.OrderBy(x => x.WhenToRemove);

            var message = string.Join('\n', ordered.Select(x => $"{x.ReminderId}: {x.TheReminder}"));
            var response = ResponseBuilder.Message(Context, message);
            
            await SendMessageAsync(response);
        }
    }
}
