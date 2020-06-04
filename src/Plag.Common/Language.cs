using System.Collections.Generic;

namespace Plag
{
    public interface ILanguage
    {
		IReadOnlyCollection<string> Suffixes { get; }

		string Name { get; }

		string ShortName { get; }

		int MinimalTokenMatch { get; }

		Structure Parse(ISubmissionFile files);

		Token CreateToken(int type, int line, int column, int length, int fileId);

		bool SupportsColumns { get; }

		bool IsPreformated { get; }

		bool UsesIndex { get; }

		int CountOfTokens { get; }

		string TypeName(int type);
	}
}
