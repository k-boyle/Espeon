using Espeon.Core;
using Espeon.Core.Services;
using Kommon.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Espeon.Services {
	public class EventsService : BaseService<InitialiseArgs>, IEventsService {
		[Inject] private readonly ILogService _logger;

		private readonly ConcurrentQueue<Func<Task>> _eventQueue;

		private CancellationTokenSource _cts;
		private int _eventCount;

		public EventsService(IServiceProvider services) : base(services) {
			this._eventQueue = new ConcurrentQueue<Func<Task>>();

			this._cts = new CancellationTokenSource();
			this._eventCount = 0;

			_ = EventQueueAsync();
		}

		//Task just to make registering cleaner
		Task IEventsService.RegisterEvent(Func<Task> @event) {
			this._eventQueue.Enqueue(@event);
			this._cts.Cancel(true);

			this._eventCount++;
			return Task.CompletedTask;
		}

		private async Task EventQueueAsync() {
			while (true) {
				if (this._eventQueue.IsEmpty) {
					try {
						await Task.Delay(-1, this._cts.Token);
					} catch (TaskCanceledException) {
						this._cts.Dispose();
						this._cts = new CancellationTokenSource();
					}
				}

				while (this._eventQueue.TryDequeue(out Func<Task> @event)) {
					try {
						await @event();
					} catch (Exception ex) {
						this._logger.Log(Source.Events, Severity.Error, string.Empty, ex);
					}
				}
			}
		}
	}
}