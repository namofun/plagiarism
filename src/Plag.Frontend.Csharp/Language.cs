﻿using Antlr4.Grammar.Csharp;
using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.IO;

namespace Xylab.PlagiarismDetect.Frontend.Csharp
{
    public class Language : ILanguage
    {
        public IReadOnlyCollection<string> Suffixes { get; } = new[] { ".CS" };

        public string Name => "C# 8.0";

        public string ShortName => "csharp";

        public int MinimalTokenMatch => 12;

        public Func<Structure, ICSharpParserListener> ListenerFactory { get; }

        public bool SupportsColumns => true;

        public bool IsPreformated => true;

        public bool UsesIndex => true;

        public int CountOfTokens => (int)TokenConstants.NUM_DIFF_TOKENS;

        public Language() : this(s => new JplagListener(s))
        {
        }

        public Language(Func<Structure, ICSharpParserListener> listenerImpl)
        {
            ListenerFactory = listenerImpl;
        }

        static Language()
        {
            CSharpLexer.InitSharedContextCache();
            CSharpParser.InitSharedContextCache();
            CSharpPreprocessorParser.InitSharedContextCache();
        }

        public Structure Parse(ISubmissionFile files)
        {
            var structure = new Structure();
            var outputWriter = new StringWriter(structure.OtherInfo);
            var errorWriter = new StringWriter(structure.ErrorInfo);
            var listener = ListenerFactory(structure);

            foreach (var item in SubmissionComposite.ExtendToLeaf(files))
            {
                var lexer = new CSharpLexer(item.Open(), outputWriter, errorWriter);
                var parser = new CSharpParser(new CommonTokenStream(lexer), outputWriter, errorWriter);
            
                parser.AddErrorListener(structure);
                parser.AddParseListener(listener);
                structure.FileId = item.Id;

                var root = parser.CompilationUnit();
                parser.ErrorListeners.Clear();
                parser.ParseListeners.Clear();
                if (!structure.EndWithEof)
                    structure.AddToken(new Token(TokenConstants.FILE_END, 0, 0, 0, item.Id));
            }

            return structure;
        }

        public string TypeName(int type) => Token.TypeToString((TokenConstants)type);

        public Frontend.Token CreateToken(int type, int line, int column, int length, int fileId)
        {
            return new Token((TokenConstants)type, line, column, column + length - 1, fileId);
        }

        public void Cleanup()
        {
            CSharpLexer.ResetSharedContextCache();
            CSharpPreprocessorParser.ResetSharedContextCache();
            CSharpParser.ResetSharedContextCache();
        }
    }
}
