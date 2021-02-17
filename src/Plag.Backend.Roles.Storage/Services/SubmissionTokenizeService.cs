using Microsoft.Extensions.DependencyInjection;
using Plag.Backend.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Plag.Backend.Services
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

        private async Task<Submission> ResolveAsync(IStoreExtService store)
        {
            var ss = await store.FetchAsync();
            if (ss == null) return null;

            var file = new Frontend.SubmissionFileProxy(ss.Files);
            var lang = Compile.FindLanguage(ss.Language);

            if (lang == null)
            {
                await store.CompileAsync(ss, "Compiler not found.", null);
            }
            else if (Compile.TryCompile(lang, file, ss.Id, out var tokens))
            {
                await store.CompileAsync(ss, "Compilation succeeded.",
                    Convert.TokenSerialize(tokens.IL));
            }
            else
            {
                await store.CompileAsync(ss,
                    $"ANTLR4 failed with {tokens.IL.ErrorsCount} errors.\r\n"
                    + tokens.IL.ErrorInfo.ToString(), null);
            }

            return ss;
        }

        private async Task ScheduleAsync(IStoreExtService store, Submission ss)
        {
            if (ss.TokenProduced != true) return;
            await store.ScheduleAsync(ss);
            AnotherSignal.Notify();
        }

        protected override async Task ProcessAsync(IStoreExtService context, CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var s = await ResolveAsync(context);
                if (s != null)
                    await ScheduleAsync(context, s);
                else
                    break;
            }
        }
    }
}
