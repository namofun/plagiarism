using Antlr4.Grammar.Cpp;
using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Plag.Frontend.Cpp
{
    public class Language : ILanguage, IAntlrErrorListener<IToken>
    {
        public IReadOnlyCollection<string> Suffixes { get; } = new[] { ".CPP", ".H", ".C", ".HPP", ".CC" };

        public string Name => "C++14";

        public string ShortName => "cpp14";

        public int MinimalTokenMatch => 12;

        public bool Errors => ErrorsCount > 0;

        public int ErrorsCount { get; private set; }

        public bool SupportsColumns => false;

        public bool IsPreformated => true;

        public bool UsesIndex => false;

        public int CountOfTokens => (int)TokenConstants.NUM_DIFF_TOKENS;

        public StringBuilder ErrorInfo { get; } = new StringBuilder();

        public StringBuilder OtherInfo { get; } = new StringBuilder();

        public Structure Parse(string fileName, Func<Stream> streamFactory)
        {
            var structure = new Structure();
            var outputWriter = new StringWriter(OtherInfo);
            var errorWriter = new StringWriter(ErrorInfo);
            var lexer = new CPP14Lexer(CharStreams.fromStream(streamFactory()), outputWriter, errorWriter);
            var parser = new CPP14Parser(new BufferedTokenStream(lexer), outputWriter, errorWriter);
            var listener = new Listener(structure);
            parser.AddErrorListener(this);
            parser.AddParseListener(listener);
            var root = parser.TranslationUnit();
            parser.ErrorListeners.Clear();
            parser.ParseListeners.Clear();
            return structure;
        }

        public void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            ErrorsCount++;
        }

        public string TypeName(int type) => Token.TypeToString((TokenConstants)type);
    }
}
