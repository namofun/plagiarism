using Microsoft.Extensions.Primitives;
using Plag.Backend.Jobs;
using System.Threading.Tasks;

namespace Plag.Backend.Services
{
    public class BackgroundServiceSignalProvider : ISignalProvider
    {
        private readonly IResettableSignal<SubmissionTokenizeService> _signal1;
        private readonly IResettableSignal<ReportGenerationService> _signal2;

        public BackgroundServiceSignalProvider(
            IResettableSignal<SubmissionTokenizeService> signal1,
            IResettableSignal<ReportGenerationService> signal2)
        {
            _signal1 = signal1;
            _signal2 = signal2;
        }

        public Task SendCompileSignalAsync()
        {
            _signal1.Notify();
            return Task.CompletedTask;
        }

        public Task SendReportSignalAsync()
        {
            _signal2.Notify();
            return Task.CompletedTask;
        }

        public Task SendRescueSignalAsync()
        {
            _signal1.Notify();
            _signal2.Notify();
            return Task.CompletedTask;
        }
    }
}
