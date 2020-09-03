#nullable enable
using Plag.Frontend;

namespace Plag.Backend.Services
{
    /// <summary>
    /// The backend service for compilation.
    /// </summary>
    public interface ICompileService
    {
        /// <summary>
        /// Find the corresponding language.
        /// </summary>
        /// <param name="name">The name of compiler.</param>
        /// <returns>The language. If not found, return <c>null</c>.</returns>
        ILanguage? FindLanguage(string name);

        /// <summary>
        /// Try to compile one submission to tokens for comparing.
        /// </summary>
        /// <param name="language">The compiler.</param>
        /// <param name="file">The submission file.</param>
        /// <param name="id">The submission ID.</param>
        /// <param name="submission">The submission produced.</param>
        /// <returns>Return whether the compilation failed.</returns>
        bool TryCompile(ILanguage language, ISubmissionFile file, string id, out Submission submission);
    }
}
