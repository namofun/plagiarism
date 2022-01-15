using Plag.Backend.Jobs;
using System.Threading;
using System.Threading.Tasks;

namespace Plag.Backend.Tests
{
    public class NullResettableSignal<T> : IResettableSignal<T>
        where T : INotifiableService
    {
        public void Dispose()
        {
        }

        public void Notify()
        {
        }

        public Task WaitAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
