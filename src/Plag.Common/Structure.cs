using Antlr4.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Xylab.PlagiarismDetect.Frontend
{
    /// <summary>
    /// Token list implemented by JPlag
    /// </summary>
    public class Structure : IAntlrErrorListener<IToken>, IReadOnlyList<Token>
    {
        private readonly List<Token> tokens = new List<Token>();

        public int HashLength { get; private set; } = -1;

        public HashTable Table { get; private set; } = null;

        public int FileId { get; set; }

        public int Files { get; private set; } = 0;

        public int Size => tokens.Count;

        public Token this[int index] => tokens[index];

        public int ErrorsCount { get; private set; }

        public bool Errors => ErrorsCount > 0;

        public StringBuilder ErrorInfo { get; } = new StringBuilder();

        public StringBuilder OtherInfo { get; } = new StringBuilder();

        public bool EndWithEof => tokens.Count == 0 || tokens[tokens.Count - 1].Type == (int)TokenConstants.FILE_END;

        public int Count => tokens.Count;

        public void EnsureNonEmpty()
        {
            if (tokens.Count == Files)
            {
                ManualError("Submission is empty.");
            }
        }

        public void ManualError(string message)
        {
            ErrorsCount++;
            ErrorInfo.AppendLine(message);
        }

        public void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            ErrorsCount++;
        }

#nullable enable
        public void CreateHashes(int? size, Func<Structure, HashTable?, int> creation)
        {
            if (HashLength != -1 && Table != null) return;
            Table = size.HasValue ? new HashTable(size.Value) : null;
            HashLength = creation(this, Table);
        }
#nullable disable

        public void AddToken(Token token)
        {
            if (tokens.Any())
            {
                var last = tokens[tokens.Count - 1];
                if (token.FileId == last.FileId)
                {
                    if (last.Line > token.Line)
                        token.Line = last.Line;
                    if (last.Column > token.Column)
                        token.Column = last.Column;
                    if (token.Length < 0)
                        token.Length = 0;
                }
            }

            tokens.Add(token);
            if (token.Type == (int)TokenConstants.FILE_END)
                Files++;
        }

        public void AddTokens(IEnumerable<Token> newcomers)
        {
            tokens.AddRange(newcomers);
            Files += tokens.Count(t => t.Type == (int)TokenConstants.FILE_END);
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

        public IEnumerator<Token> GetEnumerator()
        {
            return ((IEnumerable<Token>)tokens).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)tokens).GetEnumerator();
        }
    }
}
