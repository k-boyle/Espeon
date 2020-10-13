using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Espeon {
    public interface IOnReadyService : IHostedService {
        IServiceProvider Provider { get; }
        ILogger Logger { get; }
        
        Task OnReadyAsync(EspeonDbContext context);
        
        async Task IHostedService.StartAsync(CancellationToken cancellationToken) {
            using var scope = Provider.CreateScope();
            var bot = scope.ServiceProvider.GetRequiredService<EspeonBot>();
            Logger.Information("Waiting for ready...");
            await bot.WaitForReadyAsync();
            
            await using var context = scope.ServiceProvider.GetRequiredService<EspeonDbContext>();
            await OnReadyAsync(context);
        }

        Task IHostedService.StopAsync(CancellationToken cancellationToken) {
            return Task.CompletedTask;
        }
    }
}