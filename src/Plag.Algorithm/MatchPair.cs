namespace Xylab.PlagiarismDetect.Frontend
{
    public struct MatchPair
    {
        public readonly int StartA;
        public readonly int StartB;
        public readonly int Length;

        public MatchPair(int startA, int startB, int length)
        {
            StartA = startA;
            StartB = startB;
            Length = length;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is MatchPair match)) return false;
            return this == match;
        }

        public override int GetHashCode()
        {
            return unchecked(StartA * (Length * Length + 1) * StartB);
        }

        public static bool operator ==(MatchPair left, MatchPair right)
        {
            return left.Length == right.Length
                && left.StartA == right.StartA
                && left.StartB == right.StartB;
        }

        public static bool operator !=(MatchPair left, MatchPair right)
        {
            return !(left == right);
        }

        public bool Contains(int index, bool subA)
        {
            int start = subA ? StartA : StartB;
            return start <= index && index < (start + Length);
        }

        public bool Overlap(MatchPair other)
        {
            if (StartA < other.StartA)
            {
                if ((other.StartA - StartA) < Length) return true;
            }
            else
            {
                if ((StartA - other.StartA) < other.Length) return true;
            }

            if (StartB < other.StartB)
            {
                if ((other.StartB - StartB) < Length) return true;
            }
            else
            {
                if ((StartB - other.StartB) < other.Length) return true;
            }

            return false;
        }

        public override string ToString()
        {
            return $"{{ [{StartA}..+{Length}] x [{StartB}..+{Length}] }}";
        }
    }
}
