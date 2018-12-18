using Espeon.Core.Commands.Bases;
using Espeon.Core.Services;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Espeon.Core.Commands.Modules
{
    public abstract class Reminder : EspeonBase
    {
        public abstract IReminderService ReminderService { get; set; }

        public abstract Task CreateReminderAsync(TimeSpan when, [Remainder] string reminder);

        public abstract Task ListRemindersAsync();
    }
}
