using Casino.DependencyInjection;
using Espeon.Core.Services;
using System;

namespace Espeon.Services {
	public class DelayedCommandService : BaseService<InitialiseArgs>, IDelayedCommandService {
		public DelayedCommandService(IServiceProvider services) : base(services) { }
	}
}