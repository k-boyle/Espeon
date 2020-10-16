using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

namespace Espeon {
    public class EspeonService : IHostedService {
        private readonly EspeonBot _espeon;
        private readonly ILogger _logger;

        public EspeonService(EspeonBot espeon, ILogger logger) {
            this._espeon = espeon;
            this._logger = logger.ForContext("SourceContext", nameof(EspeonService));
        }

        public async Task StartAsync(CancellationToken cancellationToken) {
            this._logger.Information("Starting Espeon...");
            using var scope = this._espeon.CreateScope();
            await using (var context = scope.ServiceProvider.GetRequiredService<EspeonDbContext>()) {
                this._logger.Information("Migrating database...");
                await context.Database.MigrateAsync(cancellationToken: cancellationToken);
            } 
            
            _ = this._espeon.RunAsync()
                .ContinueWith(_ => this._espeon.WaitForReadyAsync(), cancellationToken)
                .ContinueWith(_ => this._logger.Information("Espeon ready"), cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken) {
            await this._espeon.StopAsync();
            await this._espeon.DisposeAsync();
        }
    }
}