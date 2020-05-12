using System;
using System.Collections.Generic;
using System.IO;

namespace Plag
{
	public interface ILanguage
    {
		IReadOnlyCollection<string> Suffixes { get; }

		string Name { get; }

		string ShortName { get; }

		int MinimalTokenMatch { get; }

		Structure Parse(string fileName, Func<Stream> streamFactory);

		bool SupportsColumns { get; }

		bool IsPreformated { get; }

		bool UsesIndex { get; }

		int CountOfTokens { get; }

		string TypeName(int type);
	}
}
