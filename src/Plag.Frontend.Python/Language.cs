using Antlr4.Grammar.Python;
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

        public Structure Parse(ISubmissionFile file)
        {
            var structure = new Structure();
            var outputWriter = new StringWriter(structure.OtherInfo);
            var errorWriter = new StringWriter(structure.ErrorInfo);
            var listener = ListenerFactory(structure);

            foreach (var item in SubmissionComposite.ExtendToLeaf(file))
            {
                var lexer = new Python3Lexer(item.Open(), outputWriter, errorWriter);
                var parser = new Python3Parser(new CommonTokenStream(lexer), outputWriter, errorWriter);

                parser.AddErrorListener(structure);
                parser.AddParseListener(listener);
                structure.FileId = item.Id;

                var root = parser.FileInput();
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
