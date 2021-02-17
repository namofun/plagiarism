using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Plag.Backend.Services
{
    public interface IResettableSignal : IDisposable
    {
        void Notify();

        Task WaitAsync(CancellationToken cancellationToken);
    }

    internal class SemaphoreSlimResettableSignal : IResettableSignal
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

    public interface IResettableSignalSource
    {
        IResettableSignal Get<T>();
    }

    internal class ResettableSignalSource<TSignal> : IResettableSignalSource
        where TSignal : IResettableSignal, new()
    {
        private readonly ConcurrentDictionary<Type, IResettableSignal> _created;

        public IResettableSignal Get<T>()
        {
            return _created.GetOrAdd(typeof(T), _ => new TSignal());
        }

        public ResettableSignalSource()
        {
            _created = new ConcurrentDictionary<Type, IResettableSignal>();
        }
    }

    public interface IResettableSignal<TCategory> : IResettableSignal
    {
    }

    internal class TypedResettableSignal<TCategory> : IResettableSignal<TCategory>
    {
        private readonly IResettableSignal _signal;

        public TypedResettableSignal(IResettableSignalSource source)
        {
            _signal = source.Get<TCategory>();
        }

        public void Dispose()
        {
            _signal.Dispose();
        }

        public void Notify()
        {
            _signal.Notify();
        }

        public Task WaitAsync(CancellationToken cancellationToken)
        {
            return _signal.WaitAsync(cancellationToken);
        }
    }
}
