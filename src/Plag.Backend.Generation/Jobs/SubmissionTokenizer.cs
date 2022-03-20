using Microsoft.Extensions.Diagnostics;
using System.Data;
using System.Threading.Tasks;
using Xylab.PlagiarismDetect.Backend.Services;
using Xylab.PlagiarismDetect.Frontend;
using Submission = Xylab.PlagiarismDetect.Backend.Models.Submission;

namespace Xylab.PlagiarismDetect.Backend.Jobs
{
    /// <summary>
    /// The submission tokenizing core logic.
    /// </summary>
    public class SubmissionTokenizer
    {
        private readonly IConvertService2 _converter;
        private readonly ICompileService _compiler;
        private readonly ITelemetryClient _telemetryClient;

        public SubmissionTokenizer(
            IConvertService2 converter,
            ICompileService compiler,
            ITelemetryClient telemetryClient)
        {
            _converter = converter;
            _compiler = compiler;
            _telemetryClient = telemetryClient;
        }

        /// <summary>
        /// Chooses one submission from queue and tokenize.
        /// </summary>
        /// <param name="store">The database context for job scheduling.</param>
        /// <returns>The tokenized submission. If no submission is proceeded, report will be ignored.</returns>
        public async Task<Submission> DoWorkAsync(IJobContext store)
        {
            var ss = await store.DequeueSubmissionAsync();
            if (ss == null) return null;

            var file = new SubmissionFileProxy(ss.Files);
            var lang = _compiler.FindLanguage(ss.Language);
            var result = await _telemetryClient.TrackScope(
                "Compile.Antlr4",
                () => Task.FromResult(Compile(lang, file, ss.ExternalId)));

            try
            {
                await store.CompileAsync(ss, result.CompileLog, result.IL);
                ss.TokenProduced = result.TokenProduced;
                return ss;
            }
            catch (DBConcurrencyException)
            {
                // Retry could cause throttling, just stop this thread.
                return null;
            }
        }

        private CompileResult Compile(ILanguage lang, ISubmissionFile file, string extId)
        {
            if (lang == null)
            {
                return new CompileResult
                {
                    TokenProduced = false,
                    CompileLog = "Compiler not found.",
                    IL = null,
                };
            }
            else if (_compiler.TryCompile(lang, file, extId, out var tokens))
            {
                return new CompileResult
                {
                    TokenProduced = true,
                    CompileLog = "Compilation succeeded.",
                    IL = _converter.TokenSerialize(tokens.IL),
                };
            }
            else
            {
                return new CompileResult
                {
                    TokenProduced = false,
                    CompileLog = $"ANTLR4 failed with {tokens.IL.ErrorsCount} errors.\r\n{tokens.IL.ErrorInfo}",
                    IL = null,
                };
            }
        }

        /// <summary>
        /// Sends cleanup signals to compilers so that GC will work.
        /// </summary>
        public void CompilerCleanup() => _compiler.Cleanup();

        private struct CompileResult
        {
            public bool TokenProduced;
            public string CompileLog;
            public byte[] IL;
        }
    }
}
