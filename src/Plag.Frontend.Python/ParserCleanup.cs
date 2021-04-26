using Antlr4.Runtime.Atn;

namespace Antlr4.Grammar.Python
{
    partial class Python3Parser
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
