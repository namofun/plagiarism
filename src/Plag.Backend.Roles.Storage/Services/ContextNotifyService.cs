using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SatelliteSite.Data;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SatelliteSite.Services
{
    public abstract class ContextNotifyService<T> : BackgroundService
    {
        static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        private IServiceProvider ServiceProvider { get; }

        public ILogger<T> Logger { get; }

        public static void Notify()
        {
            if (_semaphore.CurrentCount != 1)
                _semaphore.Release();
        }

        public ContextNotifyService(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            Logger = serviceProvider.GetRequiredService<ILogger<T>>();
        }

        protected abstract Task ProcessAsync(PlagiarismContext context, CancellationToken stoppingToken);

        protected sealed override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (true)
            {
                await _semaphore.WaitAsync(stoppingToken);
                if (stoppingToken.IsCancellationRequested) break;

                try
                {
                    using var scope = ServiceProvider.CreateScope();
                    using var dbContext = scope.ServiceProvider.GetRequiredService<PlagiarismContext>();
                    dbContext.ChangeTracker.AutoDetectChangesEnabled = false;
                    await ProcessAsync(dbContext, stoppingToken);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "An error happened unexpected.");
                }
            }
        }
    }
}
