using Antlr4.Grammar.Cpp;
using Antlr4.Runtime;
using System;

namespace Plag.Frontend.Cpp
{
    public class ErrorStrategy : DefaultErrorStrategy
    {
        protected override void ReportNoViableAlternative(Parser recognizer, NoViableAltException e)
        {
            if (e.OffendingToken.Type == CPP14Lexer.RightShift)
            {
                var o = e.OffendingToken;
                var stream = (BufferedTokenStream)recognizer.InputStream;
                var toks = stream.GetTokens();
                if (toks[toks.Count - 1] == o)
                {
                    var tok1 = recognizer.TokenFactory.Create(
                        new Tuple<ITokenSource, ICharStream>(o.TokenSource, o.InputStream),
                        CPP14Lexer.Greater, ">", o.Channel,
                        o.StartIndex, o.StartIndex, o.Line, o.Column);
                    var tok2 = recognizer.TokenFactory.Create(
                        new Tuple<ITokenSource, ICharStream>(o.TokenSource, o.InputStream),
                        CPP14Lexer.Greater, ">", o.Channel,
                        o.StopIndex, o.StopIndex, o.Line, o.Column + 1);

                    if (tok1 is IWritableToken t1 && tok2 is IWritableToken t2)
                    {
                        toks.RemoveAt(toks.Count - 1);
                        t1.TokenIndex = toks.Count;
                        toks.Add(t1);
                        t2.TokenIndex = toks.Count;
                        toks.Add(t2);
                        Recover(recognizer, e);
                        return;
                    }
                }
            }

            base.ReportNoViableAlternative(recognizer, e);
        }
    }
}
