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

        public Task StartAsync(CancellationToken cancellationToken) {
            this._logger.Information("Starting Espeon...");
            _ = this._espeon.RunAsync();
            _ = this._espeon.WaitForReadyAsync()
                .ContinueWith(_ => this._logger.Information("Espeon ready"), cancellationToken);
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken) {
            await this._espeon.StopAsync();
            await this._espeon.DisposeAsync();
        }
    }
}