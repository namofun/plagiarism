using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xylab.PlagiarismDetect.Backend.Services;

namespace Xylab.PlagiarismDetect.Backend.Jobs
{
    public class SubmissionTokenizeService : BackgroundNotifiableService<SubmissionTokenizeService>
    {
        private readonly SubmissionTokenizer _tokenizer;
        private readonly IResettableSignal<ReportGenerationService> _anotherSignal;

        public System.Timers.Timer ResetTimer { get; }

        public SubmissionTokenizeService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _tokenizer = new SubmissionTokenizer(
                serviceProvider.GetRequiredService<IConvertService2>(),
                serviceProvider.GetRequiredService<ICompileService>(),
                serviceProvider.GetRequiredService<ITelemetryClient>());

            _anotherSignal = serviceProvider.GetRequiredService<IResettableSignal<ReportGenerationService>>();

            ResetTimer = new System.Timers.Timer(15 * 1000);
            ResetTimer.Enabled = false;
            ResetTimer.AutoReset = false;
            ResetTimer.Elapsed += ResetTimer_Elapsed;
        }

        private void ResetTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            _tokenizer.CompilerCleanup();
        }

        protected override async Task ProcessAsync(IServiceScope scope, CancellationToken stoppingToken)
        {
            var context = scope.ServiceProvider.GetRequiredService<IJobContext>();
            while (!stoppingToken.IsCancellationRequested)
            {
                ResetTimer.Stop();
                var s = await _tokenizer.DoWorkAsync(context);

                if (s == null)
                {
                    ResetTimer.Start();
                    break;
                }

                if (s.TokenProduced != true) continue;
                await context.ScheduleAsync(s);
                _anotherSignal.Notify();
            }
        }
    }
}
