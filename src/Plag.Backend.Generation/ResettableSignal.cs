using System;
using System.Threading;
using System.Threading.Tasks;

namespace Plag.Backend.Services
{
    public interface IResettableSignal : IDisposable
    {
        void Notify();

        Task WaitAsync(CancellationToken cancellationToken);
    }

    public class SemaphoreSlimResettableSignal<T> : IResettableSignal<T>
        where T : IResettableService
    {
        public SemaphoreSlim Semaphore { get; }

        public SemaphoreSlimResettableSignal()
        {
            Semaphore = new SemaphoreSlim(1);
        }

        public void Notify()
        {
            if (Semaphore.CurrentCount != 1)
            {
                Semaphore.Release();
            }
        }

        public void Dispose()
        {
            Semaphore.Dispose();
        }

        public Task WaitAsync(CancellationToken cancellationToken)
        {
            return Semaphore.WaitAsync(cancellationToken);
        }
    }

    public interface IResettableSignal<TCategory> : IResettableSignal
        where TCategory : IResettableService
    {
    }

    public interface IResettableService
    {
    }
}
