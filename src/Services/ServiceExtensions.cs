using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Espeon {
    public static class ServiceExtensions {
        public static IServiceCollection AddFetchableHostedService<TService>(this IServiceCollection serviceCollection)
                where TService : class, IHostedService {
            return serviceCollection.AddSingleton<TService>()
                .AddHostedService(provider => provider.GetRequiredService<TService>());
        }
        
        public static IServiceCollection ConfigureSection<T>(this IServiceCollection collection, IConfiguration configuration) where T : class {
            return collection.Configure<T>(configuration.GetSection(typeof(T).Name));
        }
        
        public static IServiceCollection AddOnReadyService<T>(this IServiceCollection collection) where T : class, IOnReadyService {
            return collection.AddSingleton<IOnReadyService, T>()
                .AddSingleton<T>();
        }
    }
}