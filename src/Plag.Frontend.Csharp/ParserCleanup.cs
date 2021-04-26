using Antlr4.Runtime.Atn;

namespace Antlr4.Grammar.Csharp
{
    partial class CSharpParser
    {
        public static void InitSharedContextCache()
        {
            sharedContextCache = new ResettablePredictionContextCache();
        }

        public static void ResetSharedContextCache()
        {
            ((ResettablePredictionContextCache)sharedContextCache).Reset();
        }
    }

    partial class CSharpPreprocessorParser
    {
        public static void InitSharedContextCache()
        {
            sharedContextCache = new ResettablePredictionContextCache();
        }

        public static void ResetSharedContextCache()
        {
            ((ResettablePredictionContextCache)sharedContextCache).Reset();
        }
    }
}
