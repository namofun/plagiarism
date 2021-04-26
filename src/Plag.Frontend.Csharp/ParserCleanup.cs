using Antlr4.Runtime.Atn;

namespace Antlr4.Grammar.Csharp
{
    partial class CSharpParser
    {
        public static void ResetSharedContextCache()
        {
            sharedContextCache = new PredictionContextCache();
        }
    }

    partial class CSharpPreprocessorParser
    {
        public static void ResetSharedContextCache()
        {
            sharedContextCache = new PredictionContextCache();
        }
    }
}
