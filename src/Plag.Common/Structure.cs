using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Plag
{
    /// <summary>
    /// Token list implemented by JPlag
    /// </summary>
    public class Structure : IAntlrErrorListener<IToken>
    {
        private readonly List<Token> tokens = new List<Token>();

        public int HashLength { get; private set; } = -1;

        public HashTable Table { get; private set; } = null;

        public int Files { get; private set; } = 0;

        public int Size => tokens.Count;

        public Token this[int index] => tokens[index];

        public int ErrorsCount { get; private set; }

        public bool Errors => ErrorsCount > 0;

        public StringBuilder ErrorInfo { get; } = new StringBuilder();

        public StringBuilder OtherInfo { get; } = new StringBuilder();

        public bool EndWithEof => tokens.Count == 0 || tokens[^1].Type == (int)TokenConstants.FILE_END;

        public void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            ErrorsCount++;
        }

        public void CreateHashes(int? size, Func<Structure, HashTable?, int> creation)
        {
            if (HashLength != -1)
                throw new InvalidOperationException("HashTable has been built. Do this method create different results?");
            Table = size.HasValue ? new HashTable(size.Value) : null;
            HashLength = creation(this, Table);
        }

        public void AddToken(Token token)
        {
            if (tokens.Any())
            {
                var last = tokens[^1];
                if (last.Line > token.Line)
                    token.Line = last.Line;
                if (last.Column > token.Column)
                    token.Column = last.Column;
            }

            tokens.Add(token);
            if (token.Type == (int)TokenConstants.FILE_END)
                Files++;
        }

        public override string ToString()
        {
            var buf = new StringBuilder();

            try
            {
                for (int i = 0; i < Math.Min(tokens.Count, 100); i++)
                    buf.Append(i).Append('\t').AppendLine(tokens[i].ToString());
                if (tokens.Count > 100)
                    buf.AppendLine("...");
                return buf.ToString();
            }
            finally
            {
                buf.Clear();
            }
        }

        public void Save(FileInfo file) => throw new NotImplementedException();

        public bool Load(FileInfo file) => throw new NotImplementedException();
    }
}
