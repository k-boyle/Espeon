using Microsoft.Extensions.DependencyInjection;

namespace Espeon {
    public static class ServiceProviderExtensions {
        public static IServiceCollection AddInitialisableSingleton<TType>(this IServiceCollection collection) 
                where TType : class, IInitialisableService {
            return collection.AddSingleton<TType>()
                .AddSingleton<IInitialisableService>(provider => provider.GetService<TType>());
        }
    }
}