using Microsoft.Extensions.DependencyInjection;
using Plag.Backend.Models;
using Plag.Backend.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Plag.Backend.Jobs
{
    public class SubmissionTokenizeService : ContextNotifyService<SubmissionTokenizeService>
    {
        public IConvertService2 Convert { get; }

        public ICompileService Compile { get; }

        public IResettableSignal<ReportGenerationService> AnotherSignal { get; }

        public SubmissionTokenizeService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            Convert = serviceProvider.GetRequiredService<IConvertService2>();
            Compile = serviceProvider.GetRequiredService<ICompileService>();
            AnotherSignal = serviceProvider.GetRequiredService<IResettableSignal<ReportGenerationService>>();
        }

        private async Task<Submission> ResolveAsync(IJobContext store)
        {
            var ss = await store.DequeueSubmissionAsync();
            if (ss == null) return null;

            var file = new Frontend.SubmissionFileProxy(ss.Files);
            var lang = Compile.FindLanguage(ss.Language);

            if (lang == null)
            {
                await store.CompileAsync(ss.Id, "Compiler not found.", null);
            }
            else if (Compile.TryCompile(lang, file, ss.Id, out var tokens))
            {
                await store.CompileAsync(ss.Id, "Compilation succeeded.",
                    Convert.TokenSerialize(tokens.IL));
            }
            else
            {
                await store.CompileAsync(ss.Id,
                    $"ANTLR4 failed with {tokens.IL.ErrorsCount} errors.\r\n"
                    + tokens.IL.ErrorInfo.ToString(), null);
            }

            return ss;
        }

        protected override async Task ProcessAsync(IJobContext context, CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var s = await ResolveAsync(context);
                if (s == null) break;
                if (s.TokenProduced != true) continue;
                await context.ScheduleAsync(s.SetId, s.Id, s.Language);
                AnotherSignal.Notify();
            }
        }
    }
}
