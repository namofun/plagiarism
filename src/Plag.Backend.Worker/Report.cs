using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Xylab.PlagiarismDetect.Backend.Jobs;
using Xylab.PlagiarismDetect.Backend.Services;

namespace Xylab.PlagiarismDetect.Backend.Worker
{
    public class Report
    {
        private readonly IJobContext _store;
        private readonly IConvertService2 _converter;
        private readonly ICompileService _compiler;
        private readonly IReportService _reporter;
        private readonly ITelemetryClient _telemetryClient;

        public Report(
            IJobContext store,
            IConvertService2 converter,
            ICompileService compiler,
            ITelemetryClient telemetryClient,
            IReportService reporter)
        {
            _store = store;
            _compiler = compiler;
            _reporter = reporter;
            _converter = converter;
            _telemetryClient = telemetryClient;
        }

        [FunctionName("Report")]
        public Task Run(
            [QueueTrigger(Constants.ReportGeneratingQueue, Connection = "AzureWebJobsStorage")] string reportRequest,
            [Queue(Constants.ReportGeneratingQueue, Connection = "AzureWebJobsStorage")] IAsyncCollector<string> reportContinuation,
            ILogger log,
            CancellationToken cancellationToken)
        {
            return ReportWorker.RunAsync(
                reportRequest,
                _store,
                _converter,
                _compiler,
                _reporter,
                log,
                new AsyncCollectorSignalBroker(reportContinuation),
                _telemetryClient,
                cancellationToken);
        }
    }
}
