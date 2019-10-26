using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;

namespace Espeon.Core {
	public static partial class Extensions {
		public static IServiceCollection AddConfiguredHttpClient(this IServiceCollection services) {
			return services.AddHttpClient("",
				client => {
					client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
				}).Services;
		}
	}
}