using Antlr4.Runtime.Atn;

namespace Antlr4.Grammar.Cpp
{
    partial class CPP14Parser
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
