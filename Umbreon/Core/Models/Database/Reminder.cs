using System;
using System.Collections.Generic;
using LiteDB;

namespace Umbreon.Core.Models.Database
{
    public class Reminders
    {
        [BsonId]
        public int Index;
        public List<Reminder> TheReminders { get; set; } = new List<Reminder>();
    }

    public class Reminder
    {
        public string TheReminder { get; set; }
        public ulong Channel { get; set; }
        public ulong UserId { get; set; }
        public DateTime ToExecute { get; set; }
    }
}
