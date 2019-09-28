using System;
using System.Threading.Tasks;

namespace Espeon.Services
{
    public interface IEventsService
    {
        Task RegisterEvent(Func<Task> @event);
    }
}
