using Antlr4.Runtime.Atn;

namespace Antlr4.Grammar.Java
{
    partial class Java9Parser
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
