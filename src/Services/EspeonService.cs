using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Espeon {
    public class EspeonService : IHostedService {
        private readonly EspeonBot _espeon;
        private readonly ILogger<EspeonService> _logger;

        public EspeonService(EspeonBot espeon, ILogger<EspeonService> logger) {
            this._espeon = espeon;
            this._logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken) {
            this._logger.LogInformation("Starting Espeon...");
            using var scope = this._espeon.CreateScope();
            await using (var context = scope.ServiceProvider.GetRequiredService<EspeonDbContext>()) {
                this._logger.LogInformation("Migrating database...");
                await context.Database.MigrateAsync(cancellationToken: cancellationToken);
            }

            _ = this._espeon.RunAsync(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken) {
            await this._espeon.StopAsync();
            await this._espeon.DisposeAsync();
        }
    }
}