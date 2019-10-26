using System;
using System.Threading.Tasks;

namespace Espeon.Core.Services {
	public interface IEventsService {
		Task RegisterEvent(Func<Task> @event);
	}
}