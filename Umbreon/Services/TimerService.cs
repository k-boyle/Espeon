using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using Discord;
using Umbreon.Attributes;
using Umbreon.Core;
using Umbreon.Interfaces;

namespace Umbreon.Services
{
    [Service]
    public class TimerService
    {
        private readonly LogService _log;

        private Timer _timer;
        private ConcurrentQueue<IRemoveable> _queue = new ConcurrentQueue<IRemoveable>();

        public TimerService(LogService log)
        {
            _log = log;
        }

        public void InitialiseTimer()
        {
            _timer = new Timer(_ =>
            {
                if (_queue.TryDequeue(out var removeable))
                {
                    removeable.Service.Remove(removeable);
                }

                _log.NewLogEvent(LogSeverity.Verbose, LogSource.Timer, "Memory cleaned");
            }, null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        }

        public void Enqueue(IRemoveable removeable)
        {
            _queue.Enqueue(removeable);
            _queue = new ConcurrentQueue<IRemoveable>(_queue.OrderBy(x => x.When));
            if (_queue.TryPeek(out var obj))
            {
                _timer.Change(obj.When, TimeSpan.Zero);
            }
        }
    }
}
