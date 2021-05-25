using Jobs.Services;
using Microsoft.Extensions.DependencyInjection;
using Plag.Backend.Models;
using Plag.Backend.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Plag.Backend.Jobs
{
    public class SubmissionTokenizeService : BackgroundNotifiableService<SubmissionTokenizeService>
    {
        public IConvertService2 Convert { get; }

        public ICompileService Compile { get; }

        public IResettableSignal<ReportGenerationService> AnotherSignal { get; }

        public System.Timers.Timer ResetTimer { get; }

        public SubmissionTokenizeService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            Convert = serviceProvider.GetRequiredService<IConvertService2>();
            Compile = serviceProvider.GetRequiredService<ICompileService>();
            AnotherSignal = serviceProvider.GetRequiredService<IResettableSignal<ReportGenerationService>>();

            ResetTimer = new System.Timers.Timer(15 * 1000);
            ResetTimer.Enabled = false;
            ResetTimer.AutoReset = false;
            ResetTimer.Elapsed += ResetTimer_Elapsed;
        }

        private void ResetTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Compile.Cleanup();
        }

        private async Task<Submission> ResolveAsync(IJobContext store)
        {
            var ss = await store.DequeueSubmissionAsync();
            if (ss == null) return null;

            var file = new Frontend.SubmissionFileProxy(ss.Files);
            var lang = Compile.FindLanguage(ss.Language);

            if (lang == null)
            {
                await store.CompileAsync(ss.SetId, ss.Id, "Compiler not found.", null);
                ss.TokenProduced = false;
            }
            else if (Compile.TryCompile(lang, file, ss.ExternalId, out var tokens))
            {
                await store.CompileAsync(ss.SetId, ss.Id, "Compilation succeeded.",
                    Convert.TokenSerialize(tokens.IL));
                ss.TokenProduced = true;
            }
            else
            {
                await store.CompileAsync(ss.SetId, ss.Id,
                    $"ANTLR4 failed with {tokens.IL.ErrorsCount} errors.\r\n"
                    + tokens.IL.ErrorInfo.ToString(), null);
                ss.TokenProduced = false;
            }

            return ss;
        }

        protected override async Task ProcessAsync(IServiceScope scope, CancellationToken stoppingToken)
        {
            var context = scope.ServiceProvider.GetRequiredService<IJobContext>();
            while (!stoppingToken.IsCancellationRequested)
            {
                ResetTimer.Stop();
                var s = await ResolveAsync(context);

                if (s == null)
                {
                    ResetTimer.Start();
                    break;
                }

                if (s.TokenProduced != true) continue;
                await context.ScheduleAsync(s.SetId, s.Id, s.ExclusiveCategory, s.InclusiveCategory, s.Language);
                AnotherSignal.Notify();
            }
        }
    }
}
