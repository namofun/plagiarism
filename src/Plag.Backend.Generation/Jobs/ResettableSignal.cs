using System;
using System.Threading;
using System.Threading.Tasks;

namespace Plag.Backend.Jobs
{
    /// <summary>
    /// The interface for notifiable signal.
    /// </summary>
    public interface INotifiableService
    {
    }

    /// <summary>
    /// The interface for resettable signal.
    /// </summary>
    public interface IResettableSignal : IDisposable
    {
        /// <summary>
        /// Notifies the signal has flashed.
        /// </summary>
        void Notify();

        /// <summary>
        /// Waits for the signal to come.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task for waiting signal.</returns>
        Task WaitAsync(CancellationToken cancellationToken);
    }

    /// <summary>
    /// The interface for resettable signal to resolve in dependency injection container.
    /// </summary>
    /// <typeparam name="TCategory">The service type to notify.</typeparam>
    public interface IResettableSignal<TCategory> : IResettableSignal
        where TCategory : INotifiableService
    {
    }

    /// <summary>
    /// The <see cref="IResettableSignal"/> implementation using <see cref="SemaphoreSlim"/>.
    /// </summary>
    /// <typeparam name="T">The service type to notify.</typeparam>
    public sealed class SemaphoreSlimResettableSignal<T> : IResettableSignal<T>
        where T : INotifiableService
    {
        /// <summary>
        /// The core <see cref="SemaphoreSlim"/>.
        /// </summary>
        public SemaphoreSlim Semaphore { get; }

        /// <summary>
        /// Initialize the <see cref="SemaphoreSlim"/>.
        /// </summary>
        public SemaphoreSlimResettableSignal()
        {
            Semaphore = new SemaphoreSlim(1);
        }

        /// <inheritdoc />
        public void Notify()
        {
            if (Semaphore.CurrentCount != 1)
            {
                Semaphore.Release();
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Semaphore.Dispose();
        }

        /// <inheritdoc />
        public Task WaitAsync(CancellationToken cancellationToken)
        {
            return Semaphore.WaitAsync(cancellationToken);
        }
    }
}
