using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Dfa;
using System.Collections.Generic;
using System.Reflection;

namespace Antlr4
{
    public static class AntlrResettingExtensions
    {
        private readonly static FieldInfo _field =
            typeof(PredictionContextCache)
                .GetTypeInfo()
                .GetDeclaredField("cache");

        public static void Reset(this PredictionContextCache cache) =>
            _field.SetValue(
                cache,
                new Dictionary<PredictionContext, PredictionContext>());

        public static void Reset(this DFA[] dfas)
        {
            foreach (var dfa in dfas)
            {
                if (dfa.states.Count > 0)
                {
                    dfa.states = new Dictionary<DFAState, DFAState>();
                    if (dfa.IsPrecedenceDfa)
                    {
                        dfa.s0 = new DFAState(new ATNConfigSet())
                        {
                            edges = System.Array.Empty<DFAState>(),
                            isAcceptState = false,
                            requiresFullContext = false
                        };
                    }
                    else
                    {
                        dfa.s0 = null;
                    }
                }
            }
        }
    }
}
