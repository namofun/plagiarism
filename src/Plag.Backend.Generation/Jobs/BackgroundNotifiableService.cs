using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Plag.Backend.Jobs
{
    /// <summary>
    /// Base class for implementing a notifiable background service.
    /// </summary>
    /// <typeparam name="T">The service type itself.</typeparam>
    public abstract class BackgroundNotifiableService<T> : BackgroundService, INotifiableService
        where T : BackgroundNotifiableService<T>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<T> _logger;
        private readonly IResettableSignal<T> _signal;

        /// <summary>
        /// Initializes the service.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        protected BackgroundNotifiableService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _logger = serviceProvider.GetRequiredService<ILogger<T>>();
            _signal = serviceProvider.GetRequiredService<IResettableSignal<T>>();
        }

        /// <summary>
        /// Processes the works after notified.
        /// </summary>
        /// <param name="scope">The service scope.</param>
        /// <param name="stoppingToken">The stoppping token.</param>
        /// <returns>The task to execute the works.</returns>
        protected abstract Task ProcessAsync(IServiceScope scope, CancellationToken stoppingToken);

        /// <inheritdoc />
        protected sealed override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (true)
            {
                await _signal.WaitAsync(stoppingToken);
                if (stoppingToken.IsCancellationRequested) break;

                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    await ProcessAsync(scope, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error happened unexpected.");
                }
            }
        }
    }
}
