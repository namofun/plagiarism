using Antlr4.Grammar.Cpp;
using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.IO;

namespace Plag.Frontend.Cpp
{
    public class Language : LanguageImpl<JplagListener>
    {
        private static readonly Func<Structure, JplagListener> FactoryImpl = s => new JplagListener(s);

        public Language() : base(FactoryImpl) { }
    }

    public class LanguageImpl<T> : ILanguage where T : class, ICPP14Listener
    {
        public IReadOnlyCollection<string> Suffixes { get; } = new[] { ".CPP", ".H", ".C", ".HPP", ".CC" };

        public string Name => "C++14";

        public string ShortName => "cpp14";

        public int MinimalTokenMatch => 12;

        public Func<Structure, T> ListenerFactory { get; }

        public bool SupportsColumns => true;

        public bool IsPreformated => true;

        public bool UsesIndex => true;

        public int CountOfTokens => (int)TokenConstants.NUM_DIFF_TOKENS;

        public LanguageImpl(Func<Structure, T> listenerImpl)
        {
            ListenerFactory = listenerImpl;
        }

        public Structure Parse(string fileName, Func<Stream> streamFactory)
        {
            var structure = new Structure();
            var outputWriter = new StringWriter(structure.OtherInfo);
            var errorWriter = new StringWriter(structure.ErrorInfo);
            var lexer = new CPP14Lexer(CharStreams.fromStream(streamFactory()), outputWriter, errorWriter);
            var parser = new CPP14Parser(new CommonTokenStream(lexer), outputWriter, errorWriter);
            var listener = ListenerFactory(structure);
            parser.ErrorHandler = new ErrorStrategy();
            parser.AddErrorListener(structure);
            parser.AddParseListener(listener);
            var root = parser.TranslationUnit();
            parser.ErrorListeners.Clear();
            parser.ParseListeners.Clear();
            return structure;
        }

        public string TypeName(int type) => Token.TypeToString((TokenConstants)type);
    }
}
