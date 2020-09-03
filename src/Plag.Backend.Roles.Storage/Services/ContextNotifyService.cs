using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Plag.Backend.Services
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

        protected abstract Task ProcessAsync(IStoreExtService context, CancellationToken stoppingToken);

        protected sealed override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (true)
            {
                await _semaphore.WaitAsync(stoppingToken);
                if (stoppingToken.IsCancellationRequested) break;

                try
                {
                    using var scope = ServiceProvider.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<IStoreExtService>();
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
