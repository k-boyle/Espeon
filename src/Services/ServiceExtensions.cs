using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Espeon {
    public static class ServiceExtensions {
        public static IServiceCollection AddTHostedService<TService>(this IServiceCollection serviceCollection)
                where TService : class, IHostedService {
            return serviceCollection.AddSingleton<TService>()
                .AddHostedService(provider => provider.GetRequiredService<TService>());
        }
    }
}