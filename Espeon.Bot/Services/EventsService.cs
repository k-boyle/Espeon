using Casino.DependencyInjection;
using Espeon.Services;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Espeon.Bot.Services
{
    public class EventsService : BaseService<InitialiseArgs>, IEventsService
    {
        [Inject] private readonly ILogService _logger;

        private readonly ConcurrentQueue<Func<Task>> _eventQueue;

        private CancellationTokenSource _cts;
        private int _eventCount;

        public EventsService(IServiceProvider services) : base(services)
        {
            _eventQueue = new ConcurrentQueue<Func<Task>>();

            _cts = new CancellationTokenSource();
            _eventCount = 0;

            _ = EventQueueAsync();
        }

        //Task just to make registering cleaner
        Task IEventsService.RegisterEvent(Func<Task> @event)
        {
            _eventQueue.Enqueue(@event);
            _cts.Cancel(true);

            _eventCount++;
            return Task.CompletedTask;
        }

        private async Task EventQueueAsync()
        {
            while(true)
            {
                if (_eventQueue.IsEmpty)
                {
                    try
                    {
                        await Task.Delay(-1, _cts.Token);
                    }
                    catch(TaskCanceledException)
                    {
                        _cts.Dispose();
                        _cts = new CancellationTokenSource();
                    }
                }

                while (_eventQueue.TryDequeue(out var @event))
                {
                    try
                    {
                        await @event();
                    }
                    catch (Exception ex)
                    {
                        _logger.Log(Source.Events, Severity.Error, string.Empty, ex);
                    }
                }
            }
        }
    }
}
