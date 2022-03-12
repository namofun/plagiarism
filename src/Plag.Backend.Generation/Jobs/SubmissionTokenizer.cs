using System.Threading.Tasks;
using Xylab.PlagiarismDetect.Backend.Models;
using Xylab.PlagiarismDetect.Backend.Services;

namespace Xylab.PlagiarismDetect.Backend.Jobs
{
    /// <summary>
    /// The submission tokenizing core logic.
    /// </summary>
    public class SubmissionTokenizer
    {
        private readonly IConvertService2 _converter;
        private readonly ICompileService _compiler;

        public SubmissionTokenizer(IConvertService2 converter, ICompileService compiler)
        {
            _converter = converter;
            _compiler = compiler;
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

            var file = new Frontend.SubmissionFileProxy(ss.Files);
            var lang = _compiler.FindLanguage(ss.Language);

            if (lang == null)
            {
                await store.CompileAsync(ss, "Compiler not found.", null);
                ss.TokenProduced = false;
            }
            else if (_compiler.TryCompile(lang, file, ss.ExternalId, out var tokens))
            {
                await store.CompileAsync(ss, "Compilation succeeded.", _converter.TokenSerialize(tokens.IL));
                ss.TokenProduced = true;
            }
            else
            {
                await store.CompileAsync(ss, $"ANTLR4 failed with {tokens.IL.ErrorsCount} errors.\r\n{tokens.IL.ErrorInfo}", null);
                ss.TokenProduced = false;
            }

            return ss;
        }

        /// <summary>
        /// Sends cleanup signals to compilers so that GC will work.
        /// </summary>
        public void CompilerCleanup() => _compiler.Cleanup();
    }
}
