﻿using Antlr4.Grammar.Python;
using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.IO;

namespace Plag.Frontend.Python
{
    public class Language : ILanguage
    {
        public IReadOnlyCollection<string> Suffixes { get; } = new[] { ".PY" };
        
        public string Name => "Python3";

        public string ShortName => "py3";

        public int MinimalTokenMatch => 12;

        public bool SupportsColumns => true;

        public bool IsPreformated => true;

        public Func<Structure, IPython3Listener> ListenerFactory { get; }

        public bool UsesIndex => true;

        public int CountOfTokens => (int)TokenConstants.NUM_DIFF_TOKENS;

        public Language() : this(s => new JplagListener(s))
        {
        }

        public Language(Func<Structure, IPython3Listener> factory)
        {
            ListenerFactory = factory;
        }

        public Structure Parse(string fileName, Func<Stream> streamFactory)
        {
            var structure = new Structure();
            var outputWriter = new StringWriter(structure.OtherInfo);
            var errorWriter = new StringWriter(structure.ErrorInfo);
            var lexer = new Python3Lexer(CharStreams.fromStream(streamFactory()), outputWriter, errorWriter);
            var parser = new Python3Parser(new CommonTokenStream(lexer), outputWriter, errorWriter);
            var listener = ListenerFactory(structure);
            parser.AddErrorListener(structure);
            parser.AddParseListener(listener);
            var root = parser.FileInput();
            parser.ErrorListeners.Clear();
            parser.ParseListeners.Clear();
            if (!structure.EndWithEof)
                structure.AddToken(new Token(TokenConstants.FILE_END, 0, 0, 0));
            return structure;
        }

        public string TypeName(int type) => Token.TypeToString((TokenConstants)type);
    }
}
