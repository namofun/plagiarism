using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Plag.Backend.Models;
using Plag.Backend.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Plag.Backend.Jobs
{
    public class ReportGenerationService : BackgroundNotifiableService<ReportGenerationService>
    {
        private readonly ReportGenerator _generator;
        private readonly BackgroundServiceOptions _options;

        public ReportGenerationService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _generator = new ReportGenerator(
                serviceProvider.GetRequiredService<ICompileService>(),
                serviceProvider.GetRequiredService<IConvertService2>(),
                serviceProvider.GetRequiredService<IReportService>());

            _options = serviceProvider.GetOptions<BackgroundServiceOptions>().Value;

            _generator.SetLogger(serviceProvider.GetRequiredService<ILogger<ReportGenerationService>>());
        }

        protected override async Task ProcessAsync(IServiceScope scope, CancellationToken stoppingToken)
        {
            var context = scope.ServiceProvider.GetRequiredService<IJobContext>();
            var lru = new LruStore<(string, int), (Submission, Frontend.Submission)>();
            while (!stoppingToken.IsCancellationRequested)
            {
                if (!(_options.UseBatchInReport
                    ? 0 < await _generator.DoWorkBatchAsync(context, lru)
                    : await _generator.DoWorkAsync(context, lru)))
                {
                    break;
                }
            }

            lru.Clear();
        }
    }
}
