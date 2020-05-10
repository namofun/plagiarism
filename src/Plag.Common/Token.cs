namespace Plag
{
    public abstract class Token
    {
        public Token(int type, int line, int column = -1, int length = -1)
        {
            Type = type;
            Line = line > 0 ? line : 1;
            Column = column;
            Length = length;
        }

        public bool Marked { get; set; }
        public int Hash { get; set; } = -1;
        public int Type { get; internal set; }
        public virtual int Line { get; internal set; }
        public virtual int Column { get; internal set; }
        public virtual int Length { get; internal set; }

        protected virtual int Index => -1;

        public override string ToString() => "<abstract>";

        public virtual int NumberOfTokens() => 1;
    }
}
