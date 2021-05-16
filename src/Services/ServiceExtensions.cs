using System;
using System.IO;
using Disqord;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Yaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Espeon {
    public static class ServiceExtensions {
        private const string DefaultConfigDir = "./config.yml";

        public static IServiceCollection AddFetchableHostedService<TService>(this IServiceCollection serviceCollection)
                where TService : class, IHostedService {
            return serviceCollection.AddSingleton<TService>()
                .AddHostedService(provider => provider.GetRequiredService<TService>());
        }
        
        public static IServiceCollection ConfigureSection<T>(this IServiceCollection collection, IConfiguration configuration) where T : class {
            return collection.Configure<T>(configuration.GetSection(typeof(T).Name));
        }
        
        public static IServiceCollection AddOnReadyService<T>(this IServiceCollection collection) where T : class, IOnReadyService {
            return collection.AddSingleton<IOnReadyService, T>(provider => provider.GetService<T>())
                .AddSingleton<T>();
        }

        public static IServiceCollection AddEspeonDbContext(this IServiceCollection collection) {
            return collection.AddDbContext<EspeonDbContext>(
                (provider, options) => {
                    var postgresOptions = provider.GetRequiredService<IOptions<Postgres>>();
                    options.UseNpgsql(postgresOptions.Value.ConnectionString);
                },
                optionsLifetime: ServiceLifetime.Singleton);
        }

        public static IHostBuilder ConfigureEspeon(
                this IHostBuilder builder,
                Action<IServiceProvider, DiscordBotConfiguration> botConfigAction,
                Func<IServiceProvider, EspeonPrefixProvider> prefixProviderSupplier) {
            return builder.ConfigureServices((_, serviceCollection) => {
                serviceCollection.AddSingleton(provider => {
                    var espeonLogger = provider.GetRequiredService<ILogger<EspeonBot>>();
                    var discordOptions = provider.GetRequiredService<IOptions<Discord>>();
                    var prefixProvider = prefixProviderSupplier(provider);
                    var botConfig = new DiscordBotConfiguration();
                    botConfigAction(provider, botConfig);
                    botConfig.ProviderFactory = _ => provider;
                    return new EspeonBot(
                        espeonLogger,
                        discordOptions,
                        prefixProvider,
                        botConfig);
                });
            });
        }

        public static IHostBuilder ConfigureEspeonConfiguration(this IHostBuilder builder) {
            return builder.ConfigureAppConfiguration(configurationBuilder => {
                var configDir = configurationBuilder.Build()["config"] ?? DefaultConfigDir;
                if (!File.Exists(configDir)) {
                    throw new FileNotFoundException($"Missing config file {configDir}");
                }

                switch (Path.GetExtension(configDir)) {
                    case ".yaml":
                    case ".yml":
                        configurationBuilder.AddYamlFile(configDir);
                        break;

                    case ".json":
                        configurationBuilder.AddJsonFile(configDir);
                        break;

                    default:
                        throw new InvalidOperationException($"{Path.GetExtension(configDir)} is not a valid config type");
                }
            });
        }
    }
}