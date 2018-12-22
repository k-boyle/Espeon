using Espeon.Core.Services;
using Qmmands;
using System;
using System.Linq;
using System.Threading.Tasks;
using Base = Espeon.Core.Commands.Modules;

namespace Espeon.Commands.Modules
{
    [Name("Reminder")]
    [Group("Reminder")]
    public class Reminder : Base.Reminder
    {
        public override IReminderService ReminderService { get; set; }

        [Command]
        [Name("Reminder")]
        public override async Task CreateReminderAsync(TimeSpan when, [Remainder] string reminder)
        {
            await ReminderService.CreateReminderAsync(Context, reminder, when);

            var response = await Response.GetResponseAsync(Module, Command, ResponsePack);
            var embed = ResponseBuilder.Message(Context, response);

            await SendMessageAsync(embed);
        }

        //TODO Make better
        [Command("List")]
        [Name("List Reminders")]
        public override async Task ListRemindersAsync()
        {
            var reminders = await ReminderService.GetRemindersAsync(Context);
            var ordered = reminders.OrderBy(x => x.WhenToRemove);

            var message = string.Join('\n', ordered.Select(x => $"{x.Id}: {x.TheReminder}"));
            var response = ResponseBuilder.Message(Context, message);
            
            await SendMessageAsync(response);
        }
    }
}
