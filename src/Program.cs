using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Espeon {
    internal class Program {
        private static void WriteEspeonAscii() {
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine(Constants.EspeonAscii);
            Console.ForegroundColor = default;
        }

        private static async Task Main(string[] args) {
            WriteEspeonAscii();
            var host = CreateHostBuilder(args).Build();
            using var scope = host.Services.CreateScope();
            await host.RunAsync();
        }
        
        private static IHostBuilder CreateHostBuilder(string[] args) {
            return Host.CreateDefaultBuilder(args)
                .UseSerilog((hostContext, loggingConfiguration) => {
                    loggingConfiguration.ReadFrom.Configuration(hostContext.Configuration);
                })
                .ConfigureEspeonConfiguration()
                .ConfigureEspeon(
                    (provider, config) => config.Logger = new DisqordLogger(provider),
                    provider => new EspeonPrefixProvider(provider.GetRequiredService<PrefixService>()))
                .ConfigureServices((hostContext, serviceCollection) => {
                    var configuration = hostContext.Configuration;
                    serviceCollection.AddSingleton<PrefixService>()
                        .AddSingleton<EspeonScheduler>()
                        .AddSingleton<HttpClient>()
                        .AddSingleton<ILocalisationProvider, PropertyBasedLocalisationProvider>()
                        .AddOnReadyService<ReminderService>()
                        .AddHostedService<EspeonService>()
                        .AddFetchableHostedService<LocalisationService>()
                        .AddEspeonDbContext()
                        .ConfigureSection<Discord>(configuration)
                        .ConfigureSection<Localisation>(configuration)
                        .ConfigureSection<Postgres>(configuration)
                        .ConfigureSection<Emotes>(configuration);
                });
        }
    }
}
