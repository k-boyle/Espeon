using Espeon.Core.Services;
using Kommon.DependencyInjection;
using System;

namespace Espeon.Services {
	public class DelayedCommandService : BaseService<InitialiseArgs>, IDelayedCommandService {
		public DelayedCommandService(IServiceProvider services) : base(services) { }
	}
}