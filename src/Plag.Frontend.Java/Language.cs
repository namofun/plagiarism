using Antlr4.Grammar.Java;
using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Plag.Frontend.Java
{
    public class Language : ILanguage
    {
        public IReadOnlyCollection<string> Suffixes { get; } = new[] { ".JAVA" };
        
        public string Name => "Java9";

        public string ShortName => "java9";

        public int MinimalTokenMatch => 9;

        public bool SupportsColumns => true;

        public bool IsPreformated => true;

        public Func<Structure, IJava9Listener> ListenerFactory { get; }

        public bool UsesIndex => true;

        public int CountOfTokens => (int)TokenConstants.NUM_DIFF_TOKENS;

        public Language() : this(s => new JplagListener(s))
        {
        }

        public Language(Func<Structure, IJava9Listener> factory)
        {
            ListenerFactory = factory;
        }

        public Structure Parse(ISubmissionFile file)
        {
            var structure = new Structure();
            var outputWriter = new StringWriter(structure.OtherInfo);
            var errorWriter = new StringWriter(structure.ErrorInfo);
            var listener = ListenerFactory(structure);

            foreach (var item in SubmissionComposite.ExtendToLeaf(file))
            {
                var lexer = new Java9Lexer(item.Open(), outputWriter, errorWriter);
                var parser = new Java9Parser(new CommonTokenStream(lexer), outputWriter, errorWriter);

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

        public Plag.Frontend.Token CreateToken(int type, int line, int column, int length, int fileId)
        {
            return new Token((TokenConstants)type, line, column, column + length - 1, fileId);
        }
    }
}
