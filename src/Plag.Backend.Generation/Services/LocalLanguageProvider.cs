using Plag.Backend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Plag.Backend.Services
{
    public class LocalLanguageProvider : ILanguageProvider
    {
        private readonly ICompileService _compiler;

        public LocalLanguageProvider(ICompileService compiler)
        {
            _compiler = compiler;
            FrontendVersion = typeof(Frontend.ILanguage).Assembly.GetName().Version.ToString();
        }

        public string FrontendVersion { get; }

        public Task<LanguageInfo> FindLanguageAsync(string langName)
        {
            var lang = _compiler.FindLanguage(langName);
            if (lang == null) return Task.FromResult<LanguageInfo>(null);
            return Task.FromResult(new LanguageInfo(lang.Name, lang.ShortName, lang.Suffixes));
        }

        public Task<List<LanguageInfo>> ListLanguageAsync()
        {
            return Task.FromResult(
                _compiler.GetLanguages()
                    .Select(a => new LanguageInfo(a.Name, a.ShortName, a.Suffixes))
                    .ToList());
        }

        public Task UpdateLanguagesAsync(List<LanguageInfo> languageSeeds)
        {
            throw new NotSupportedException("Language is runtime provided, cannot update.");
        }
    }
}
