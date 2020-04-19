using Microsoft.Extensions.DependencyInjection;

namespace Espeon {
    public static class ServiceProviderExtensions {
        public static IServiceCollection AddInitialisableSingleton<TType>(this IServiceCollection collection) 
                where TType : class, IInitialisableService {
            return collection.AddSingleton<TType>()
                .AddSingleton<IInitialisableService>(provider => provider.GetService<TType>());
        }
        
        public static IServiceCollection AddOnReadySingleton<TType>(this IServiceCollection collection) 
                where TType : class, IOnReadyService {
            return collection.AddSingleton<TType>()
                .AddSingleton<IOnReadyService>(provider => provider.GetService<TType>());
        }
    }
}