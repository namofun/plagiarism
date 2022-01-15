#nullable enable

using Plag.Backend.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Plag.Backend.Services
{
    /// <summary>
    /// The language provider.
    /// </summary>
    public interface ILanguageProvider
    {
        /// <summary>
        /// Checks whether a language exists.
        /// </summary>
        /// <param name="langName">The name of language.</param>
        /// <returns>The existence of language.</returns>
        Task<LanguageInfo?> FindLanguageAsync(string langName);

        /// <summary>
        /// Lists existing languages.
        /// </summary>
        /// <returns>The existing languages.</returns>
        Task<List<LanguageInfo>> ListLanguageAsync();

        /// <summary>
        /// Updates the languages list.
        /// </summary>
        /// <param name="languageSeeds">The languages list.</param>
        Task UpdateLanguagesAsync(List<LanguageInfo> languageSeeds);

        /// <summary>
        /// Compiler frontend version
        /// </summary>
        string FrontendVersion { get; }
    }
}
