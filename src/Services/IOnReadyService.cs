using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Espeon {
    public interface IOnReadyService<out T> : IHostedService {
        IServiceProvider Provider { get; }
        ILogger<T> Logger { get; }
        
        Task OnReadyAsync(EspeonDbContext context);
        
        async Task IHostedService.StartAsync(CancellationToken cancellationToken) {
            using var scope = Provider.CreateScope();
            var bot = scope.ServiceProvider.GetRequiredService<EspeonBot>();
            Logger.LogInformation("Waiting for ready...");
            await bot.WaitForReadyAsync();
            
            await using var context = scope.ServiceProvider.GetRequiredService<EspeonDbContext>();
            await OnReadyAsync(context);
        }

        Task IHostedService.StopAsync(CancellationToken cancellationToken) {
            return Task.CompletedTask;
        }
    }
}